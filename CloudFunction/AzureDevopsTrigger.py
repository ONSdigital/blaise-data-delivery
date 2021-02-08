import requests
import json
import os

pat_token = os.getenv("AZDO_PERSONAL_ACCESS_TOKEN")
env_name = os.getenv("AZURE_AGENT_ENVNAME")
var_group_name = os.getenv("PROJECT_ID")

def getStatus():
    request = requests.get(
        f"https://dev.azure.com/blaise-gcp/csharp/_apis/pipelines/46/runs/{pipelines_run_id}?api-version=6.0-preview.1",
        auth=("", pat_token),
        headers={"content-type": "application/json"},
    )

    last_run = json.loads(request.text)
    run_result = ""
    if "inProgress" not in last_run["state"]:
        run_result = last_run["result"]

    return last_run["state"], run_result


variables = {"VarGroup": var_group_name, "Environment": env_name}
data = {"templateParameters": variables}
request = requests.post(
    "https://dev.azure.com/blaise-gcp/csharp/_apis/pipelines/46/runs?api-version=6.0-preview.1",
    auth=("", pat_token),
    data=json.dumps(data),
    headers={"content-type": "application/json"},
)

pipeline_runs = json.loads(request.text)
print(pipeline_runs)
pipelines_run_id = pipeline_runs["id"]

wait_for_success = True
while wait_for_success:
    state, result = getStatus()
    if "completed" in state:
        print(f"Result of pipeline is {result}")
        print(pyfiglet.print_figlet(f"Result: {result}"))
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
