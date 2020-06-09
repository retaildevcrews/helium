#!/bin/bash

if [ -z "$He_Name" ]
then
  echo "Please set He_Name before running this script"
else
  if [ -f ~/helium.env ]
  then
    if [ "$#" = 0 ] || [ $1 != "-y" ]
    then
      read -p "helium.env already exists. Do you want to overright? (y/n) " response

      if ! [[ $response =~ [yY] ]]
      then
        echo "Please move or delete ~/helium.env and rerun the script."
        exit 1;
      fi
    fi
  fi

  echo '#!/bin/bash' > ~/helium.env
  echo '' >> ~/helium.env
  echo 'echo "Loading Helium keys from Key Vault"' >> ~/helium.env
  echo '' >> ~/helium.env

  # export He_Name (already set)
  echo "export He_Name=$He_Name" >> ~/helium.env
  echo '' >> ~/helium.env

  # export He_* if set
  [ "$He_ACR_Id" ] && echo "export He_ACR_Id=$He_ACR_Id" >> ~/helium.env
  [ "$He_ACR_RG" ] && echo "export He_ACR_RG=$He_ACR_RG" >> ~/helium.env
  [ "$He_App_RG" ] && echo "export He_App_RG=$He_App_RG" >> ~/helium.env
  [ "$He_Language" ] && echo "export He_Language=$He_Language" >> ~/helium.env
  [ "$He_Location" ] && echo "export He_Location=$He_Location" >> ~/helium.env
  [ "$He_SP_ID" ] && echo "export He_SP_ID=$He_SP_ID" >> ~/helium.env

  echo '' >> ~/helium.env

  # export Imdb_* if set
  [ "$Imdb_Col" ] && echo "export Imdb_Col=$Imdb_Col" >> ~/helium.env
  [ "$Imdb_DB" ] && echo "export Imdb_DB=$Imdb_DB" >> ~/helium.env
  [ "$Imdb_Name" ] && echo "export Imdb_Name=$Imdb_Name" >> ~/helium.env
  [ "$Imdb_RG" ] && echo "export Imdb_RG=$Imdb_RG" >> ~/helium.env

  echo '' >> ~/helium.env

  # Save keys to keyvault
  # export command to load values from Key Vault

  if [ "$Imdb_Key" ];
  then
    echo 'export Imdb_Key="az keyvault secret show --vault-name $He_Name --name Imdb-Key --query value -o tsv"' >> ~/helium.env
  fi

  if [ "$Imdb_Cosmos_RO_Key" ];
  then
    echo 'export Imdb_Cosmos_RO_Key="az keyvault secret show --vault-name $He_Name --name Imdb-Cosmos-RO-Key --query value -o tsv"' >> ~/helium.env
  fi

  if [ "$He_AppInsights_Key" ];
  then
    echo 'export He_AppInsights_Key="az keyvault secret show --vault-name $He_Name --name He-AppInsights-Key --query value -o tsv"' >> ~/helium.env
  fi

  if [ "$He_SP_PWD" ];
  then
    echo 'export He_SP_PWD="az keyvault secret show --vault-name $He_Name --name "He-SP-PWD" --query value -o tsv"' >> ~/helium.env
  fi

  cat ~/helium.env
fi


# az keyvault secret set --vault-name $He_Name --name "Imdb-Key" --value $(eval az keyvault secret show --vault-name $He_Name --name Imdb-Key --query value -o tsv) --query name -o tsv
# az keyvault secret set --vault-name $He_Name --name "Imdb-Cosmos-RO-Key" --value $Imdb_Cosmos_RO_Key --query name -o tsv
# az keyvault secret set --vault-name $He_Name --name "He-AppInsights-Key" --value $He_AppInsights_Key --query name -o tsv
# az keyvault secret set --vault-name $He_Name --name "He-SP-PWD" --value $He_SP_PWD --query name -o tsv
