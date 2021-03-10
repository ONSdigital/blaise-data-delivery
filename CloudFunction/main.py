import requests
import json
import os
import time

pat_token = os.getenv("pat_token")
env_name = os.getenv("env_name")
var_group_name = os.getenv("var_group_name")
git_branch = os.getenv("git_branch", "main")

def getStatus(pipeline_id):
    request = requests.get(
        f"https://dev.azure.com/blaise-gcp/csharp/_apis/pipelines/46/runs/{pipeline_id}?api-version=6.0-preview.1",
        auth=("", pat_token),
        headers={"content-type": "application/json"},
    )

    last_run = json.loads(request.text)
    run_result = ""
    if "inProgress" not in last_run["state"]:
        run_result = last_run["result"]

    return last_run["state"], run_result

def dataDelivery(data, context):
    request = requests.post(
        f"https://dev.azure.com/blaise-gcp/csharp/_apis/pipelines/46/runs?api-version=6.0-preview.1",
        auth=("", pat_token),
        json={
            "resources": {
                "repositories": {"self": {"refName": f"refs/heads/{git_branch}"}}
            },
            "templateParameters": {"VarGroup": var_group_name, "Environment": env_name},
        },
    )

    pipeline_runs = json.loads(request.text)
    print(pipeline_runs)
    pipelines_run_id = pipeline_runs["id"]

    wait_for_success = True
    while wait_for_success:
        state, result = getStatus(pipelines_run_id)
        if "completed" in state:
            print(f"Result of pipeline is {result}")
            print(f"Result: {result}")
            wait_for_success = False
            if "failed" in result:
                print("------------------------------------------------------------")
                print("    Time to boot up the old ON-NET to see what happened")
                print("------------------------------------------------------------")
                exit(1)
        else:
            print("Waiting for completed state ...")
            time.sleep(30)

    print("Result returned")
    