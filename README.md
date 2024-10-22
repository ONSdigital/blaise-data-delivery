# Blaise Data Delivery

This repository contains an Azure DevOps pipeline and associated scripts for delivering survey data. The process is triggered by Concourse jobs and utilises various components to securely transfer data from Blaise to on-premises locations.

## Configuration

Survey configurations are defined in JSON files within the `configurations` folder. Here's the standard default configuration:

```
{
    "deliver" : {
        "spss" : true,
        "ascii": true,
        "json" : false,
        "xml" : false
    },

    "createSubFolder" : false,
    "packageExtension" : "zip",
    "auditTrailData" : true,
    "batchSize" : 0,
    "throttleLimit" : 3
}
```

## Configuration settings

| Setting | Description |
| --- | --- |
| deliver | Specifies which file formats to include in the delivered package. |
| spss | Metadata in SPSS format. |
| ascii | Data in ascii format, used for SPSS. |
| json | Data in JSON format. |
| xml | Metadata in XML format. |
| createSubFolder | If true, creates a timestamped subfolder for the non-Blaise delivery formats. |
| packageExtension | Determines the package file extension (e.g., zip). |
| auditTrailData | If true, includes a CSV file containing audit trail information. |
| batchSize | Sets the maximum number of cases per batch (0 for all records). |
| throttleLimit | Limits the number of concurrently processed questionnaires. |

## Setting up new survey

The default configuration delivers the Blaise data along with SPSS and ASCII formats. To customise the configuration for a survey, create a JSON file named `<survey>.json` in the `configurations` folder, where `<survey>` is the survey type acronym (e.g., OPN, LM, IPS).

## High level data delivery process

1. Concourse job is triggered on schedule or manually.
1. Job passes survey name and Azure DevOps pipeline ID to another pipeline, which calls shell scripts.
1. Shell script calls Python script.
1. Python script initiates Azure DevOps pipeline via secure HTTP request.
1. Azure DevOps pipeline runs data delivery YAML on a dedicated VM via agent.
1. YAML executes scripts, referencing survey-specific config JSON.
1. PowerShell scripts set up the environment (Blaise license, Manipula).
1. Blaise-CLI fetches survey data using NuGet package.
1. Manipula generates various data formats (CSV, JSON, SPSS, ASCII).
1. PowerShell zips data and places it in NiFi staging bucket.
1. Cloud function encrypts the zip and moves it to the NiFi bucket.
1, Another cloud function publishes zip metadata to Pub/Sub.
1. NiFi monitors the Pub/Sub topic.
1. NiFi consumes the message, unzips the data, and delivers it on-premises.
