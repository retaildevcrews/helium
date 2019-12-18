# Setup CI-CD with ACR (Azure Container Registry) Task

Azure Conainer Registry has the ability to do Docker builds directly from a GitHub repository. ACR can also do this as a task that can be triggered based on a schedule, webhook or git commit.

## Create a GitHub personal access token

To trigger a task on a commit to a Git repository, ACR Tasks need a personal access token (PAT) to access the repository. If you do not already have a PAT, follow these steps to generate one in GitHub:

1. Navigate to the PAT creation page on GitHub at <https://github.com/settings/tokens/new>

2. Enter a short description for the token, for example, "ACR Tasks Demo"

3. Select scopes for ACR to access the repo. To access a public repo as in this tutorial, under repo, enable repo:status and public_repo

 >Note: To generate a PAT to access a private repo, select the scope for full repo control.

4. Select the Generate token button (you may be asked to confirm your password)

5. Copy and save the generated token in a secure location (you use this token when you define a task in the following section)


## Create the build task

Fork the repository you want to work with. This example is using the <https://github.com/retaildevcrews/helium-csharp> repository.

Once you have forkedthe repository then clone it to your local system

```shell
git clone http://github.com/gituser/helium-chsarp
```

Now that you've completed the steps required to enable ACR Tasks to read commit status and create webhooks in a repository, you can create a task that triggers a container image build on commits to the repo.

First, populate these shell environment variables with values appropriate for your environment. This step isn't strictly required, but makes executing the multiline Azure CLI commands in this tutorial a bit easier. If you don't populate these environment variables, you must manually replace each value wherever it appears in the example commands.

```shell
ACR_NAME=<registry-name>        # The name of your Azure container registry
GIT_USER=<github-username>      # Your GitHub user account name
GIT_PAT=<personal-access-token> # The PAT you generated in the previous section
REPO_NAME=<repo to be built>    # The Repository that has the source code and Dockerfile to build the image
```

Now, create the task by executing the following az acr task create command:

```shell
az acr task create \
    --registry $ACR_NAME \
    --name $REPO_NAME-build \
    --image $REPO_NAME \
    --context https://github.com/$GIT_USER/$REPO_NAME.git \
    --file Dockerfile \
    --git-access-token $GIT_PAT
 ```

This task specifies that any time code is committed to the master branch in the repository specified by --context, ACR Tasks will build the container image from the code in that branch. The Dockerfile specified by --file from the repository root is used to build the image. The --image argument can specify a parameterized value of {{.Run.ID}} for the version portion of the image's tag, ensuring the built image correlates to a specific build, and is tagged uniquely. This must be reflected correctly where the image is being pulled from.

Output from a successful az acr task create command is similar to the following:

```console
{
  "agentConfiguration": {
    "cpu": 2
  },
  "creationDate": "2018-09-14T22:42:32.972298+00:00",
  "id": "/subscriptions/<Subscription ID>/resourceGroups/myregistry/providers/Microsoft.ContainerRegistry/registries/myregistry/tasks/helium-csharp-build",
  "location": "westcentralus",
  "name": "helium-csharp-build",
  "platform": {
    "architecture": "amd64",
    "os": "Linux",
    "variant": null
  },
  "provisioningState": "Succeeded",
  "resourceGroup": "myregistry",
  "status": "Enabled",
  "step": {
    "arguments": [],
    "baseImageDependencies": null,
    "contextPath": "https://github.com/gituser/helium-csharp.git",
    "dockerFilePath": "Dockerfile",
    "imageNames": [
      "helloworld:{{.Run.ID}}"
    ],
    "isPushEnabled": true,
    "noCache": false,
    "type": "Docker"
  },
  "tags": null,
  "timeout": 3600,
  "trigger": {
    "baseImageTrigger": {
      "baseImageTriggerType": "Runtime",
      "name": "defaultBaseimageTriggerName",
      "status": "Enabled"
    },
    "sourceTriggers": [
      {
        "name": "defaultSourceTriggerName",
        "sourceRepository": {
          "branch": "master",
          "repositoryUrl": "https://github.com/gituser/helium-csharp",
          "sourceControlAuthProperties": null,
          "sourceControlType": "GitHub"
        },
        "sourceTriggerEvents": [
          "commit"
        ],
        "status": "Enabled"
      }
    ]
  },
  "type": "Microsoft.ContainerRegistry/registries/tasks"
}
```

## Test the build task

You now have a task that defines your build. To test the build pipeline, trigger a build manually by executing the az acr task run command:

```shell
az acr task run --registry $ACR_NAME --name helium-csharp-build
```

By default, the az acr task run command streams the log output to your console when you execute the command.

```shell
az acr task run --registry $ACR_NAME --name helium-csharp-build
2019/12/11 15:33:18 Downloading source code...
2019/12/11 15:33:20 Finished downloading source code
2019/12/11 15:33:21 Using acb_vol_df1899fb-9097-4edc-8698-010a308bb1f4 as the home volume
2019/12/11 15:33:21 Setting up Docker configuration...
2019/12/11 15:33:22 Successfully set up Docker configuration
2019/12/11 15:33:22 Logging in to registry: myregejv.azurecr.io
2019/12/11 15:33:23 Successfully logged into myregejv.azurecr.io
2019/12/11 15:33:23 Executing step ID: build. Timeout(sec): 28800, Working directory: '', Network: ''
2019/12/11 15:33:23 Scanning for dependencies...
2019/12/11 15:33:24 Successfully scanned dependencies
2019/12/11 15:33:24 Launching container with name: build
Sending build context to Docker daemon  2.171MB
Step 1/13 : FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
2.2: Pulling from dotnet/core/sdk
Digest: sha256:3ad18424d43e58cfc03ff828465a811463add47042de3ae0d1aeffbef97fd63d
Status: Downloaded newer image for mcr.microsoft.com/dotnet/core/sdk:2.2
 ---> 84120c6c3491
Step 2/13 : COPY src /src
 ---> 46ab1751d2cf
Step 3/13 : WORKDIR /src/unit-tests
 ---> Running in 0a7aa72ee7c3
Removing intermediate container 0a7aa72ee7c3
 ---> f9a0155bafdf
Step 4/13 : RUN dotnet test --logger:trx
 ---> Running in 08e7c6caa98f
Test run for /src/unit-tests/bin/Debug/netcoreapp2.2/unit-tests.dll(.NETCoreApp,Version=v2.2)
Microsoft (R) Test Execution Command Line Tool Version 16.0.1
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
Results File: /src/unit-tests/TestResults/_08e7c6caa98f_2019-12-11_15_33_53_087.trx

Total tests: 17. Passed: 17. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: 3.6850 Seconds
Removing intermediate container 08e7c6caa98f
 ---> 4b81f9b92c3a
Step 5/13 : WORKDIR /src/app
 ---> Running in 5ae762937b8d
Removing intermediate container 5ae762937b8d
 ---> 1f9906749135
Step 6/13 : RUN dotnet publish -c Release -o /app
 ---> Running in a9c911599d8c
Microsoft (R) Build Engine version 16.0.450+ga8dc7f1d34 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restore completed in 111.39 ms for /src/app/helium.csproj.
  Restore completed in 51.15 ms for /src/app/helium.csproj.
  helium -> /src/app/bin/Release/netcoreapp2.2/helium.dll
  helium -> /app/
Removing intermediate container a9c911599d8c
 ---> 611e55ac8be0
Step 7/13 : FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
2.2: Pulling from dotnet/core/aspnet
Digest: sha256:470b07b57e48fc0b742f0498e7e4a1e697c17002317a33bd0dae0b9e6c6bc8ad
Status: Downloaded newer image for mcr.microsoft.com/dotnet/core/aspnet:2.2
 ---> 594143f47344
Step 8/13 : EXPOSE 4120
 ---> Running in 09e2364fec96
Removing intermediate container 09e2364fec96
 ---> 3ca723fd1ca1
Step 9/13 : WORKDIR /app
 ---> Running in 474baa1f0e11
Removing intermediate container 474baa1f0e11
 ---> 98c426308d00
Step 10/13 : RUN groupadd -g 4120 helium &&     useradd -r  -u 4120 -g helium helium &&     mkdir -p /home/helium &&     chown -R helium:helium /home/helium
 ---> Running in 1729b44dde16
Removing intermediate container 1729b44dde16
 ---> 7a66e67cae0d
Step 11/13 : USER helium
 ---> Running in f3ed2c7d3041
Removing intermediate container f3ed2c7d3041
 ---> 47dba89aa94c
Step 12/13 : COPY --from=build /app .
 ---> 84a42ac314cf
Step 13/13 : ENTRYPOINT [ "dotnet",  "helium.dll" ]
 ---> Running in ab774ad43fb5
Removing intermediate container ab774ad43fb5
 ---> 4aa71d1353a2
Successfully built 4aa71d1353a2
Successfully tagged myregejv.azurecr.io/helium-csharp:cj11
2019/12/11 15:34:18 Successfully executed container: build
2019/12/11 15:34:18 Executing step ID: push. Timeout(sec): 1800, Working directory: '', Network: ''
2019/12/11 15:34:18 Pushing image: myregejv.azurecr.io/helium-csharp:cj11, attempt 1
The push refers to repository [myregejv.azurecr.io/helium-csharp]
3fd9e227cead: Preparing
5c3bb1089f30: Preparing
8281019f21b7: Preparing
5788d77a8c40: Preparing
a2f9ed91e120: Preparing
75bb365bb264: Preparing
99b5261d397c: Preparing
75bb365bb264: Waiting
99b5261d397c: Waiting
a2f9ed91e120: Layer already exists
5788d77a8c40: Layer already exists
75bb365bb264: Layer already exists
99b5261d397c: Layer already exists
8281019f21b7: Pushed
5c3bb1089f30: Pushed
3fd9e227cead: Pushed
cj11: digest: sha256:5c28202fe57c2fe289e3dd60b2a5b95a60eef657c2e0394fb87275b924fcf5e4 size: 1789
2019/12/11 15:34:27 Successfully pushed image: myregejv.azurecr.io/helium-csharp:cj11
2019/12/11 15:34:27 Step ID: build marked as successful (elapsed time in seconds: 54.801341)
2019/12/11 15:34:27 Populating digests for step ID: build...
2019/12/11 15:34:30 Successfully populated digests for step ID: build
2019/12/11 15:34:30 Step ID: push marked as successful (elapsed time in seconds: 9.178506)
2019/12/11 15:34:30 The following dependencies were found:
2019/12/11 15:34:30
- image:
    registry: myregejv.azurecr.io
    repository: helium-csharp
    tag: cj11
    digest: sha256:5c28202fe57c2fe289e3dd60b2a5b95a60eef657c2e0394fb87275b924fcf5e4
  runtime-dependency:
    registry: mcr.microsoft.com
    repository: dotnet/core/aspnet
    tag: "2.2"
    digest: sha256:470b07b57e48fc0b742f0498e7e4a1e697c17002317a33bd0dae0b9e6c6bc8ad
  buildtime-dependency:
  - registry: mcr.microsoft.com
    repository: dotnet/core/sdk
    tag: "2.2"
    digest: sha256:3ad18424d43e58cfc03ff828465a811463add47042de3ae0d1aeffbef97fd63d
  git:
    git-head-revision: ec1f8ee097c4797aee35f22643f1e0ce3e90490d

Run ID: cj11 was successful after 1m12s
```

Trigger a build with a commit

Now that you've tested the task by manually running it, trigger it automatically with a source code change.

First, ensure you're in the directory containing your local clone of the repository. Make a small change that is inconsequential to the your local copy of the clones repository and then commit the change back to origin. AS an example add a blank line to the appsettings.json of the helium-csharp src code

```shell
git add appsettings.json
git commit -m "Testing CI"
git push origin master
Username for 'https://github.com': <github-username>
Password for 'https://githubuser@github.com': <personal-access-token>
```

Once you've pushed a commit to your repository, the webhook created by ACR Tasks fires and kicks off a build in Azure Container Registry. Display the logs for the currently running task to verify and monitor the build progress:

```shell
az acr task logs --registry $ACR_NAME
```

Output is similar to the following, showing the currently executing (or last-executed) task:

```shell
az acr task logs --registry $ACR_NAME
Showing logs of the last created run.
Run ID: da4

[...]

Run ID: da4 was successful after 38s
```

List builds
To see a list of the task runs that ACR Tasks has completed for your registry, run the az acr task list-runs command:

```shell
az acr task list-runs --registry $ACR_NAME --output table
```

Output from the command should appear similar to the following. The runs that ACR Tasks has executed are displayed, and "Git Commit" appears in the TRIGGER column for the most recent task:

```shell
az acr task list-runs --registry $ACR_NAME --output table

RUN ID    TASK                  PLATFORM    STATUS     TRIGGER     STARTED               DURATION
--------  --------------        ----------  ---------  ----------  --------------------  ----------
da4       helium-csharp-build   Linux       Succeeded  Git Commit  2018-09-17T23:03:45Z  00:00:44
da3       helium-csharp-build   Linux       Succeeded  Manual      2018-09-17T22:55:35Z  00:00:35
da2       helium-csharp-build   Linux       Succeeded  Manual      2018-09-17T22:50:59Z  00:00:32
da1                             Linux       Succeeded  Manual      2018-09-17T22:29:59Z  00:00:57
```
