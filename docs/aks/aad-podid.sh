#!/bin/sh
while getopts :a:r:m: option
do
 case "${option}" in
 a) AKS_NAME=${OPTARG};;
 r) AKS_RG=${OPTARG};;
 m) MI_NAME=${OPTARG};;
 *) echo "Please refer to usage guide on GitHub" >&2
    exit 1 ;;
 esac
done



echo "creating required variables"
if echo $AKS_NAME > /dev/null 2>&1 && echo $AKS_RG > /dev/null 2>&1; then
    if ! export AKS_RES_ID=$(az aks show -g ${AKS_RG} -n ${AKS_NAME} --query id -o tsv); then
        echo "ERROR: failed to get resource ID or AKS Cluster"
        exit 1
    fi
    echo "AKS Resource ID = $AKS_RES_ID"
fi

if echo $AKS_NAME > /dev/null 2>&1 && echo $AKS_RG > /dev/null 2>&1; then
    if ! export AKS_NODE_RG=$(az aks show -g ${AKS_RG} -n ${AKS_NAME} --query nodeResourceGroup -o tsv); then
        echo "ERROR: failed to get Node Resource Group for AKS Cluster"
        exit 1
    fi
    echo "AKS Node Resource Group = $AKS_NODE_RG"
fi

if echo $AKS_NODE_RG > /dev/null 2>&1; then
    if ! export AKS_NODE_RG_RESID=$(az group show -n ${AKS_NODE_RG} --query id -o tsv); then
        echo "ERROR: failed to get Node Resource Group Resource ID for AKS Cluster"
        exit 1
    fi
    echo "AKS Node Resource Group Full ID = $AKS_NODE_RG_RESID"
fi

echo "creating azure user assigned managed identity $MI_NAME in the $AKS_NODE_RG Resource Group"
if echo $AKS_NODE_RG > /dev/null 2>&1 && echo $MI_NAME > /dev/null 2>&1; then
    if ! export MI_PrincID=$(az identity create -g ${AKS_NODE_RG} -n ${MI_NAME} --query principalId -o tsv); then
        echo "ERROR: failed to create managed identity"
        exit 1
    fi
    echo "Managed Identity Principal ID = $MI_PrincID"
fi

echo "getting azure user assigned managed identity $MI_NAME resource ID"
if echo $AKS_NODE_RG > /dev/null 2>&1 && echo $MI_NAME > /dev/null 2>&1; then
    if ! export MI_ResID=$(az identity show -g ${AKS_NODE_RG} -n ${MI_NAME} --query id -o tsv); then
        echo "ERROR: failed to get managed identity resource ID"
        exit 1
    fi
    echo "Managed Identity Resource ID = $MI_ResID"
fi

echo "getting azure user assigned managed identity $MI_NAME client ID"
if echo $AKS_NODE_RG > /dev/null 2>&1 && echo $MI_NAME > /dev/null 2>&1; then
    if ! export MI_ClientID=$(az identity show -g ${AKS_NODE_RG} -n ${MI_NAME} --query clientId -o tsv); then
        echo "ERROR: failed to get managed identity resource ID"
        exit 1
    fi
    echo "Managed Identity Client ID = $MI_ClientID"
fi

while ! az role assignment create --role Reader --assignee $MI_PrincID --scope $AKS_NODE_RG_RESID
do
  echo "Sleeping for 10 seconds waiting for AAD Propogation of Identity"
  sleep 10s
done

echo "assigning the managed identity Principal ID $MI_PrincID reader role to  the $AKS_NODE_RG Resource Group"
if echo $MI_PrincID > /dev/null 2>&1 && echo $AKS_NODE_RG > /dev/null 2>&1; then
    if ! az role assignment create --role Reader --assignee $MI_PrincID --scope $AKS_NODE_RG_RESID; then
        echo "ERROR: failed to assign the reader role to the managed identity"
        exit 1
    fi
fi

echo "creating required variables"
if echo $AKS_NAME > /dev/null 2>&1 && echo $AKS_RG >/dev/null 2>&1; then
    if ! export AKS_SPID=$(az aks show -g ${AKS_RG} -n ${AKS_NAME} --query 'servicePrincipalProfile.clientId' -o tsv); then
        echo "ERROR: failed to get Service Principal ID for AKS Cluster"
        exit 1
    fi
    echo "AKS Service Principal ID = $AKS_SPID"
fi

if echo $AKS_NAME > /dev/null 2>&1 && echo $AKS_RG >/dev/null 2>&1; then
    if ! export AKS_SP_ObjID=$(az ad sp show --id ${AKS_SPID} --query objectId -o tsv); then
        echo "ERROR: failed to get Service Principal Object ID for AKS Cluster"
        exit 1
    fi
    echo "AKS Service Principal Object ID = $AKS_SP_ObjID"
fi

echo "assigning the AKS Service Principal Managed Identity Operator rights"
if echo $AKS_SP_ObjID > /dev/null 2>&1 && echo $MI_ResID > /dev/null 2>&1; then
    if ! az role assignment create --role "Managed Identity Operator" --assignee $AKS_SP_ObjID --scope $MI_ResID; then
        echo "ERROR: failed to assign the AKS Service Principal Managed Identity Operator rights"
        exit 1
    fi
fi

# echo "assigning the Managed Identity access rights to the Azure Key Vault"
# if echo $KEY_VAULT_NAME > /dev/null 2>&1 && echo $MI_PrincID > /dev/null 2>&1; then
#     if ! az keyvault set-policy -n $KEY_VAULT_NAME --object-id $MI_PrincID --secret-permissions get list --key-permissions get list --certificate-permissions get list; then
#         echo "ERROR: failed to assign the Managed Identity access rights to the Azure Key Vault"
#         exit 1
#     fi
# fi

# write aad helm chart values file.
cat << EOF > cluster/manifests/aadpodidentity/${MI_NAME}-values.yaml
azureIdentities:
  - name: "${MI_NAME}"
    namespace: "default"
    type: 0
    resourceID: "${MI_ResID}"
    clientID: "${MI_ClientID}"
    binding:
      name: "${MI_NAME}-binding"
      selector: "${MI_NAME}"
EOF

echo "creating aad-pod-identity deployment in the default namespace with values file "
if ! kubectl get deploy mic > /dev/null 2>&1; then
    if ! helm install aad-pod-identity aad-pod-identity/aad-pod-identity -f cluster/manifests/aadpodidentity/${MI_NAME}-values.yaml; then
        echo "ERROR: failed to create kubernetes aad-pod-idenity deployment"
        exit 1
    fi
fi

echo " "
echo "******************************************************************************************************************"
echo "AAD Pod Identity has been deployed to you cluster $AKS_NAME and is using $MI_NAME for its managed Identity"
echo "Now you can configure ${MI_NAME} to have access rights to any azure resource based on Azure IAM roles"
echo "******************************************************************************************************************"
echo " "
echo "******************************************************************************************************************"
echo "To assign the MI to your keyvault run the following:"
echo "az keyvault set-policy -n ${He_Name} --object-id ${MI_PrincID} --secret-permissions get list \\"
echo "--key-permissions get list --certificate-permissions get list"
echo "******************************************************************************************************************"
echo " "
echo "******************************************************************************************************************"
echo "Add the label below to your deployment to assign an aad MI to those pods:"
echo " aadpodidentity: ${MI_NAME}"
echo "******************************************************************************************************************"
