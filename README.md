# Blaise Data Delivery 🚚

This repository contains the Azure DevOps pipeline definitions and supporting scripts for delivering survey questionnaire data from Blaise. The process is initiated by a Concourse job, which triggers the Azure DevOps pipeline described here. Multiple output formats and configuration options are supported, allowing flexible delivery of survey data in formats such as SPSS, ASCII, JSON, and XML. All scripts and configuration files required for this process are included in this repository.

While this repository covers the automation and packaging of data for delivery, the final transfer of files to on-premises locations is handled by NiFi and additional downstream processes, which are outside the scope of this repository. Several further steps occur after the processes defined here to complete the end-to-end data delivery.

Survey configurations are managed using JSON files in the `configurations` folder. If no survey-specific configuration is provided, the `default.json` file is used, which delivers Blaise data along with SPSS and ASCII formats. To customise the delivery settings for a particular survey, create a JSON file named `<survey>.json` in the `configurations` folder, where `<survey>` is the survey acronym (e.g., OPN, LM, IPS). This allows you to specify unique configuration options for each survey.

The Manipula source scripts and their compiled counterparts are included directly in this repository. The data delivery process does not recompile Manipula scripts each time it runs; instead, it executes the pre-compiled versions already present in the repo. If any changes are made to the Manipula source files, they must be recompiled manually, and both the updated source and compiled files should be committed to the repository. This approach ensures consistent and reliable execution of the data delivery process, while making it clear that updates to Manipula logic require explicit recompilation and version control.

## Configuration settings

| Setting | Description |
| --- | --- |
| deliver | Specifies which file formats to include in the delivered package. |
| asciiData | Data in ASCII (.ASC) format, used for SPSS. Also creates "remarks" CSV file (.FPS). |
| jsonData | Data in JSON format. Only includes populated fields. |
| spssMetadata | Metadata in SPSS (.SPS) format. |
| xmlData | Data in XML format. |
| xmlMetadata | Metadata in XML format. |
| createSubFolder | If true, creates a timestamped subfolder for the non-Blaise delivery formats. Allows for retention of each delivery. |
| auditTrailData | If true, includes a CSV file containing audit trail data. |
| packageExtension | Determines the package file extension (e.g., zip). |
| batchSize | Sets the maximum number of cases per batch (0 for all). |
| throttleLimit | Limits the number of concurrently processed questionnaires. |

## High-level data delivery process

1. Concourse job is triggered on schedule or manually.
1. Job passes survey acronym and Azure DevOps pipeline ID to another pipeline, which calls shell scripts.
1. Shell script calls Python script.
1. Python script initiates Azure DevOps pipeline via secure HTTP request.
1. Azure DevOps pipeline runs data delivery YAML on a dedicated VM via agent.
1. YAML executes scripts, referencing survey-specific config JSON.
1. PowerShell scripts set up the environment (Blaise license, download Manipula, download 7-Zip, set-up processing folder).
1. Blaise-CLI fetches survey data using NuGet package.
1. Manipula scripts generates various data formats depending on configuration (SPSS, ASCII, JSON, XML).
1. PowerShell zips data and places it in NiFi staging bucket.
1. Cloud function encrypts the zip and moves it to the NiFi bucket.
1. Another cloud function publishes zip metadata to Pub/Sub topic.
1. NiFi monitors the Pub/Sub topic.
1. NiFi consumes the message, un-zips the data, and delivers it on-premises.

## Sandbox data delivery

To enable sandbox data delivery, a sandbox specific data delivery configuration must first be set-up in the Concourse pipeline, or the Azure DevOps data delivery pipeline run manually from the Azure DevOps console and sandbox details passed to the pipeline. NiFi is not configured for sandbox environments and cannot deliver data on-premises from them directly. The data delivery zip file is initially uploaded to the sandbox NiFi bucket. A Cloud Function then copies the file to the dev NiFi bucket, where NiFi is configured to process and deliver files on-premises. The Cloud Function also renames the zip file to include the name of the originating sandbox environment. This ensures the file can be easily identified and avoids conflicts with standard dev environment deliveries. Once in the dev bucket, the usual data delivery workflow resumes.

## Sandbox data delivery process

1. Data delivery zip file uploaded to sandbox NiFi bucket.
1. Cloud function is triggered and checks if the filename starts with `dd`.
1. Zip file renamed to include the sandbox environment suffix, e.g. `dd_<survey>_sandbox_<env_suffix>_<timestamp>.zip`
1. Renamed file copied into the dev NiFi bucket.
1. Usual data delivery process kicks in to deliver the data on-premises.
