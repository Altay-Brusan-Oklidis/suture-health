# Mirth Connect Channel to transmit HL7 messages

This is for transmitting HL7 messages. It currently transmits MDM message to S3 bucket and Kinesis data stream.

## Steps to run channel
1. Import channel.
    - Run mirth connect administrator and choose "Channels" in right panel inside the "Mirth Connect".
    <img src="mirth\images\choose_channel.png">
    - Then click "Import channel" inside the "Channel Tasks".
    <img src="mirth\images\channel_import.png">
    - Choose the "hl7.xml" file. You can find this file in the "mirth/channel" folder. Then click "open" button.
    <img src="mirth\images\import_dialog.png">
    - Then it will ask to import code template library and click "Yes".
    <img src="mirth\images\template_dialog.png">

2. Build the jar file.
    - Build the jar file from the java code in the "src" folder.
    - To build this code you may need dependencies and you can find all the dependencis in the s3 bucket "s3://hl7messagebucket/packages/".

3. Import jar files.
    - To use extra jar files you should import it and it's dependencies.
    - Go to settings/Resources in mirth connect administrator.
    <img src="mirth\images\resource_tab.png">
    - Choose "Add Resource" and change the name to "MirthAWS" (you can doubleclick to change the name).
    <img src="mirth\images\add_resource.png">
    - Make a new folder and copy the jar file(built in second step) and dependencies.
    - Copy that folder that to mirth connect.
    <img src="mirth\images\set_directory.png">
    - Save it and reload resource.
    <img src="mirth\images\reload_resource.png">
    - Go to "hl7" channel and edit it.
    <img src="mirth\images\edit_channel.png">
    - In the summary tab  and click "set dependency" button.
    - In the Library resources tab check "MirthAWS" and click "OK" button.
    <img src="mirth\images\set_dependency.png">

4. Set environment variables.
    - Go to "settings/Configuration Map" and Add values.
    - You should add "s3BucketName" as "hl7messagebucket", "kinesisDataStreamName" as "HL7DataStream", "accessKey" as "aws access key", "secretKey" as "aws access secret key".
    <img src="mirth\images\configuration_map.png">

5. Deploy the channel.
    - Got to "hl7" channel and edit it. Then go to source tab.
    - Then change Dicretory to the foler path of raw MDM messages.
    <img src="mirth\images\source_tab.png">
    - Then go to "Destination" tab.
    - Edit "AWS Access Key ID", and "AWS Secret Access Key" with you information in "SaveRawMDMMessageOnS3Bucket", "SaveMDMMessageOnS3BucketAsJson", "SaveMDMMessageOnS3BucketWithoutPdfAsJson" destination.
    <img src="mirth\images\destination_tab.png">
    - Then click "Deploy Channel" to deploy and start the channel.
    - After some time you may see this result.
    <img src="mirth\images\deploy_result.png">

## Run MirthConnect using docker.
- Build Docker Image.
    ```
    docker build -t mirthconnect .
    ```
- Run MirthConnect with postgresql.
    ```
    docker-compose up
    ```
- Import Channels from "mirth/channel/HL7.xml".
- Set environment variables. Look at Step 4 in "Steps to run channel" .