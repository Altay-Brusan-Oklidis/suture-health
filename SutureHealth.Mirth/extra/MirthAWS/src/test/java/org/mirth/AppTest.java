package org.mirth;

import static org.junit.Assert.assertTrue;

import org.junit.Test;

/**
 * Unit test for simple App.
 */
public class AppTest 
{
    /**
     * Rigorous Test :-)
     */
    @Test
    public void shouldAnswerWithTrue()
    {
        //configurationMap.kinesisDataStreamLoggerName_test, connectorMessage.getEncodedData(), "ADT", "1", configurationMap.accessKey, configurationMap.secretKey
        String accessKey = "YOUR_AWS_ACCESS_KEY";
        String HCHB_Security_Key ="YOUR_HCHB_SECURITY_KEY";
        String kinesisDataStreamLoggerName_test ="test-lambda-kinesis";
        String kinesisDataStreamName_test ="HL7DataStream-Test";
        String secretKey = "YOUR_AWS_SECRET_KEY";
        String encodedData = "MSH|^~\\&|SutureHealth|testing|hchb||202305090458||ADT^A01^ADT_A01|202305091658JNQDSSEW|P|2.5\n" +
                "EVN|A01|198610140000|202205220000|01||202305091658\n" +
                "PID|1|ER-BK-3716|DB-24^^^HCHB&HCHB&ISO^PN~LM-26^^^HCHB&HCHB&ISO^PI|ZJ-50^^^HCHB&HCHB&ISO^PI|Nicholson^Jane^TN||20130408|M|||Horseshoe Ct^^Baltimore^New Hampshire^06880^^P^Q2BSA^368|||||||FHP8M7S72Z0C|587-95-1942|||||||||||N\n" +
                "PV1||O\n" +
                "IN1|1|1969873208|ACMEIns|Hershel^D|^^Los Angeles^^94920^USA|||||||||||^KP6YS||19870331|Happy Hollow Rd^^Los Angeles^Florida^94920|||||||||||||||||PQHG3";
        String result = MirthAWS.sendMDMToKinesis(kinesisDataStreamName_test,encodedData,"ADT", "1",accessKey,secretKey);

        assertTrue( true );
    }
}
