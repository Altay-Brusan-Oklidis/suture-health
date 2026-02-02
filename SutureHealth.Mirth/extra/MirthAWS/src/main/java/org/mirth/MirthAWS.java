package org.mirth;

import java.io.File;
import java.io.FileOutputStream;
import java.nio.ByteBuffer;
import java.util.Base64;

import com.amazonaws.auth.AWSCredentials;
import com.amazonaws.auth.BasicAWSCredentials;
import com.amazonaws.auth.AWSStaticCredentialsProvider;
import com.amazonaws.regions.Regions;
import com.amazonaws.services.kinesis.AmazonKinesis;
import com.amazonaws.services.kinesis.AmazonKinesisClientBuilder;
import com.amazonaws.services.kinesis.model.PutRecordRequest;
import com.amazonaws.services.kinesis.model.PutRecordResult;
import com.amazonaws.services.s3.AmazonS3;
import com.amazonaws.services.s3.AmazonS3ClientBuilder;
import com.amazonaws.services.s3.model.PutObjectRequest;


public class MirthAWS {
	
	public static void sendExtractPdfToS3Bucket(String bucketName, String key, String encodedContent, String accessKey, String secretKey){
	    // Decode the Base-64 data and store it as a PDF file
	    byte[] pdfData = Base64.getDecoder().decode(encodedContent);
	    
		try { 
			// write a content to temp file
			File tempFile = File.createTempFile("temp", "pdf");
		    FileOutputStream fos = new FileOutputStream(tempFile);
		    fos.write(pdfData);
		    fos.close();
		    
		    AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
		    AmazonS3 bucket = AmazonS3ClientBuilder.standard()
		    									   .withCredentials(new AWSStaticCredentialsProvider(credentials))
		    									   .withRegion(Regions.US_EAST_1)
		    									   .build();
		    PutObjectRequest request = new PutObjectRequest(bucketName, key, tempFile);
		    bucket.putObject(request);
		    
		} catch (Exception e) {
			System.out.println(e.toString());
		}
	}
	
	public static String sendMDMToKinesis(String dataStreamName, String messageContent, String partitionKey, String sequenceNumberOfPreviousRecord, String accessKey, String secretKey) {
		AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
		AmazonKinesis kinesisClient = AmazonKinesisClientBuilder
												   .standard()
												   .withCredentials(new AWSStaticCredentialsProvider(credentials))
												   .withRegion(Regions.US_EAST_1)
												   .build();
		
		PutRecordRequest putRecordRequest = new PutRecordRequest();
		putRecordRequest.setStreamName( dataStreamName );
		putRecordRequest.setData(ByteBuffer.wrap( messageContent.getBytes() ));
		putRecordRequest.setPartitionKey( partitionKey );  
		putRecordRequest.setSequenceNumberForOrdering( sequenceNumberOfPreviousRecord );
		try {
			PutRecordResult putRecordResult = kinesisClient.putRecord( putRecordRequest );
			sequenceNumberOfPreviousRecord = putRecordResult.getSequenceNumber();
			return sequenceNumberOfPreviousRecord;
		} catch (Exception e) {
			return e.toString();
		}
		
	}
	
}
