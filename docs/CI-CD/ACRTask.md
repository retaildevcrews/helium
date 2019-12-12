# Setup CI-CD with ACR (Azure Container Registry) Task
Azure Conainer Registry has the ability to do Docker builds directly from a GitHub repository. ACR can also do this as a task that can be triggered based on a schedule, webhook or git commit.

## Create a GitHub personal access token
To trigger a task on a commit to a Git repository, ACR Tasks need a personal access token (PAT) to access the repository. If you do not already have a PAT, follow these steps to generate one in GitHub:

1. Navigate to the PAT creation page on GitHub at https://github.com/settings/tokens/new

2. Enter a short description for the token, for example, "ACR Tasks Demo"

3. Select scopes for ACR to access the repo. To access a public repo as in this tutorial, under repo, enable repo:status and public_repo

Screenshot of the Personal Access Token generation page in GitHub

 >Note: To generate a PAT to access a private repo, select the scope for full repo control.

4. Select the Generate token button (you may be asked to confirm your password)

5. Copy and save the generated token in a secure location (you use this token when you define a task in the following section)

Screenshot of the generated Personal Access Token in GitHub

## Create the build task
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
  "id": "/subscriptions/<Subscription ID>/resourceGroups/myregistry/providers/Microsoft.ContainerRegistry/registries/myregistry/tasks/taskhelloworld",
  "location": "westcentralus",
  "name": "taskhelloworld",
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
    "contextPath": "https://github.com/gituser/acr-build-helloworld-node",
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
          "repositoryUrl": "https://github.com/gituser/acr-build-helloworld-node",
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
az acr task run --registry $ACR_NAME --name taskhelloworld
```

By default, the az acr task run command streams the log output to your console when you execute the command.

```shell
$ az acr task run --registry $ACR_NAME --name taskhelloworld

2018/09/17 22:51:00 Using acb_vol_9ee1f28c-4fd4-43c8-a651-f0ed027bbf0e as the home volume
2018/09/17 22:51:00 Setting up Docker configuration...
2018/09/17 22:51:02 Successfully set up Docker configuration
2018/09/17 22:51:02 Logging in to registry: myregistry.azurecr.io
2018/09/17 22:51:03 Successfully logged in
2018/09/17 22:51:03 Executing step: build
2018/09/17 22:51:03 Obtaining source code and scanning for dependencies...
2018/09/17 22:51:05 Successfully obtained source code and scanned for dependencies
Sending build context to Docker daemon  23.04kB
Step 1/5 : FROM node:9-alpine
9-alpine: Pulling from library/node
Digest: sha256:8dafc0968fb4d62834d9b826d85a8feecc69bd72cd51723c62c7db67c6dec6fa
Status: Image is up to date for node:9-alpine
 ---> a56170f59699
Step 2/5 : COPY . /src
 ---> 5f574fcf5816
Step 3/5 : RUN cd /src && npm install
 ---> Running in b1bca3b5f4fc
npm notice created a lockfile as package-lock.json. You should commit this file.
npm WARN helloworld@1.0.0 No repository field.

up to date in 0.078s
Removing intermediate container b1bca3b5f4fc
 ---> 44457db20dac
Step 4/5 : EXPOSE 80
 ---> Running in 9e6f63ec612f
Removing intermediate container 9e6f63ec612f
 ---> 74c3e8ea0d98
Step 5/5 : CMD ["node", "/src/server.js"]
 ---> Running in 7382eea2a56a
Removing intermediate container 7382eea2a56a
 ---> e33cd684027b
Successfully built e33cd684027b
Successfully tagged myregistry.azurecr.io/helloworld:da2
2018/09/17 22:51:11 Executing step: push
2018/09/17 22:51:11 Pushing image: myregistry.azurecr.io/helloworld:da2, attempt 1
The push refers to repository [myregistry.azurecr.io/helloworld]
4a853682c993: Preparing
[...]
4a853682c993: Pushed
[...]
da2: digest: sha256:c24e62fd848544a5a87f06ea60109dbef9624d03b1124bfe03e1d2c11fd62419 size: 1366
2018/09/17 22:51:21 Successfully pushed image: myregistry.azurecr.io/helloworld:da2
2018/09/17 22:51:21 Step id: build marked as successful (elapsed time in seconds: 7.198937)
2018/09/17 22:51:21 Populating digests for step id: build...
2018/09/17 22:51:22 Successfully populated digests for step id: build
2018/09/17 22:51:22 Step id: push marked as successful (elapsed time in seconds: 10.180456)
The following dependencies were found:
- image:
    registry: myregistry.azurecr.io
    repository: helloworld
    tag: da2
    digest: sha256:c24e62fd848544a5a87f06ea60109dbef9624d03b1124bfe03e1d2c11fd62419
  runtime-dependency:
    registry: registry.hub.docker.com
    repository: library/node
    tag: 9-alpine
    digest: sha256:8dafc0968fb4d62834d9b826d85a8feecc69bd72cd51723c62c7db67c6dec6fa
  git:
    git-head-revision: 68cdf2a37cdae0873b8e2f1c4d80ca60541029bf


Run ID: da2 was successful after 27s
Trigger a build with a commit
```

Now that you've tested the task by manually running it, trigger it automatically with a source code change.

First, ensure you're in the directory containing your local clone of the repository. Make a small change the is inconsequential to the your local copy of the clones repository and then commit the change back to origin

```shell
$ git push origin master
Username for 'https://github.com': <github-username>
Password for 'https://githubuser@github.com': <personal-access-token>
```

Once you've pushed a commit to your repository, the webhook created by ACR Tasks fires and kicks off a build in Azure Container Registry. Display the logs for the currently running task to verify and monitor the build progress:

```shell
az acr task logs --registry $ACR_NAME
```

Output is similar to the following, showing the currently executing (or last-executed) task:

```shell
$ az acr task logs --registry $ACR_NAME
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
$ az acr task list-runs --registry $ACR_NAME --output table

RUN ID    TASK                  PLATFORM    STATUS     TRIGGER     STARTED               DURATION
--------  --------------        ----------  ---------  ----------  --------------------  ----------
da4       helium-csharp-build   Linux       Succeeded  Git Commit  2018-09-17T23:03:45Z  00:00:44
da3       helium-csharp-build   Linux       Succeeded  Manual      2018-09-17T22:55:35Z  00:00:35
da2       helium-csharp-build   Linux       Succeeded  Manual      2018-09-17T22:50:59Z  00:00:32
da1                             Linux       Succeeded  Manual      2018-09-17T22:29:59Z  00:00:57
```
