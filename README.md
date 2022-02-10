# Blaise-Data-Delivery

## usage 
This repository consists of an Azure DevOps pipeline represented by the "data_delivery_pipeline.yml" yaml file, and a series of powershell scripts. The responsibility of the scripts is to take a survey type as a parameter and is then responsable for delivering all data for that survey based upon the configuartion set for the survey type.

## Concourse triggers

## NIFI

## Manipula

## Configuration settings
The configuration of the delivery files and formats for an instrument is defined in a JSON file contained in the configurations folder. There are currently six options in the following format:

```
{
    "deliver" : {
        "spss" : true,
        "ascii": true,
        "json" : false,
        "xml" : false
    },

    "createSubFolder" : false,
    "packageExtension" : "zip"
}
```

The first four options are contained in the "deliver" section of the JSOn file. This section is responsable for determining what data formats are included in the delivered package. The file format options are:

1. spss => 
2. ascii => 
3. json => 
4. xml =>

The other two options in the configuration file are:

5. createSubFolder => If this is set to true, then all the files configured within the "deliver" section will be contained in a subfolder of the package.
6. packageExtension => This determines the file extension of the package that is delivered

## Setting up a new instrument for delivery
The default configuration for an instrument will deliver spss and ascii files only and will not use a subfolder. If you wish to use a custom cofiguration for your instrument that deviates from the default, then you simply need to create a new JSON configuration file in the configuration folder and name it <survey>.json where survey type is the acronym for the survey i.e. OPN, LM, NWO.
