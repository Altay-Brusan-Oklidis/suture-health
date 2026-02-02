using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SutureHealth.Hchb.JsonConverters;
using SutureHealth.Hchb.Services.Testing.Model.Request;
using SutureHealth.Requests;
using SutureHealth.Requests.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Text;

namespace SutureHealth.Hchb.Services.Testing
{
    public class HchbMessageSerializerTest
    {

        [Test]
        public void CreateAdtMessageFromJsonStringShouldSuccessfullyDeserializeControlId()
        {
            // Init
            string message = "{\"messageControlId\":\"2023091508534504100062192501\",\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\",\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\",\"patient\":{\"FirstName\":\"PEGGY\",\"middleName\":\"S\",\"LastName\":\"MCNEIL\",\"suffix\":\"\",\"BirthDate\":\"1936-02-04\",\"Gender\":\"F\",\"addressLine1\":\"4860 HARVARD CT\",\"addressLine2\":\"\",\"city\":\"BURLINGTON\",\"state\":\"KY\",\"postalCode\":\"41005\",\"identifiers\":[]},\"type\":\"A01\",\"branchCode\":\"041\",\"patient_hchb\":{\"externalId\":\"\",\"PatientId\":\"621925\",\"admissionId\":\"\",\"episodeId\":\"1113498\",\"status\":\"CURRENT\",\"physicianNpi\":\"1811173685\",\"physicianFirstName\":\"KATHLEEN\",\"physicianLastName\":\"OBRIEN\"},\"rawfilename\":\"ADT/raw/ADT-A01-2023091508534504100062192501.txt\",\"jsonfilename\":\"ADT/json/ADT-A01-2023091508534504100062192501.json\"}";
            // Act
            Adt adt = new Adt(message);
            // Assert
            Assert.IsTrue(adt.MessageControlId == "2023091508534504100062192501");
        }

        [Test]
        public void CreateAdtMessageFromJsonStringWithMultiplePatientIdShouldSuccessfullyDeserialize()
        {
            // Init
            string message = "{\"messageControlId\":\"202309150854170E200062125001\",\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\",\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\",\"patient\":{\"FirstName\":\"TANYA\",\"middleName\":\"\",\"LastName\":\"GOODY\",\"suffix\":\"\",\"BirthDate\":\"1949-09-26\",\"Gender\":\"F\",\"addressLine1\":\"3624 S J ST\",\"addressLine2\":\"\",\"city\":\"TACOMA\",\"state\":\"WA\",\"postalCode\":\"98418\",\"identifiers\":[{\"type\":\"mbi\",\"value\":\"7CP3AX2JC71\"}]},\"type\":\"A01\",\"branchCode\":\"0E2\",\"patient_hchb\":{\"externalId\":\"\",\"PatientId\":\"621250\",\"admissionId\":\"\",\"episodeId\":\"1112203\",\"status\":\"CURRENT\",\"physicianNpi\":\"1588611313\",\"physicianFirstName\":\"CHARLES\",\"physicianLastName\":\"NAM\"},\"rawfilename\":\"ADT/raw/ADT-A01-202309150854170E200062125001.txt\",\"jsonfilename\":\"ADT/json/ADT-A01-202309150854170E200062125001.json\"}";
            // Act
            Adt adt = new Adt(message);
            // Assert
            Assert.IsTrue(adt.Patient.Identifiers.FirstOrDefault()?.Type == "mbi" &&
                          adt.Patient.Identifiers.FirstOrDefault()?.Value == "7CP3AX2JC71");
        }

        [Test]
        public void CreateAdtMessageFromJsonStringShouldSuccessfullyDeserializeAllfields()
        {
            // Init
            string message = "{" +
                "\"messageControlId\":\"2023091508541804000059888601\"," +
                "\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "\"patient\":" +
                "  {" +
                "   \"firstName\":\"RAYMOND\"," +
                "   \"middleName\":\"A\"," +
                "   \"lastName\":\"STERWERF\"," +
                "   \"suffix\":\"\"," +
                "   \"birthDate\":\"1927-03-02\"," +
                "   \"gender\":\"M\"," +
                "   \"addressLine1\":\"3283 SOUTH RD\"," +
                "   \"addressLine2\":\"\"," +
                "   \"city\":\"WESTWOOD\"," +
                "   \"state\":\"OH\"," +
                "   \"postalCode\":\"45248\"," +
                "   \"identifiers\":" +
                "    [" +
                "       {" +
                "         \"type\":\"ssn\"," +
                "         \"value\":\"291221562\"" +
                "       }," +
                "       {" +
                "         \"type\":\"ssn4\"," +
                "         \"value\":\"1562\"" +
                "       }," +
                "       {" +
                "         \"type\":\"mbi\"," +
                "         \"value\":\"7JP4A74DV66\"" +
                "       }" +
                "   ]" +
                "}," +
                "\"type\":\"A01\"," +
                "\"branchCode\":\"040\"," +
                "\"patient_hchb\"" +
                "   :{" +
                "       \"externalId\":\"\"," +
                "       \"patientId\":\"598886\"," +
                "       \"admissionId\":\"\"," +
                "       \"episodeId\":\"1114264\"," +
                "       \"status\":\"CURRENT\"," +
                "       \"physicianNpi\":\"1518904879\"," +
                "       \"physicianFirstName\":\"JOSEPH\"," +
                "       \"physicianLastName\":\"SEIBERT\"," +
                "       \"icd10code\":\"8790\"" +
                "    }," +
                "\"rawfilename\":\"ADT/raw/ADT-A01-2023091508541804000059888601.txt\"," +
                "\"jsonfilename\":\"ADT/json/ADT-A01-2023091508541804000059888601.json\"}";

            // Act
            Adt adt = new Adt(message);

            // Assert
            Assert.IsTrue(adt.MessageControlId == "2023091508541804000059888601");

            Assert.IsTrue(adt.Patient.FirstName == "RAYMOND");
            Assert.IsTrue(adt.Patient.MiddleName == "A");
            Assert.IsTrue(adt.Patient.LastName == "STERWERF");
            Assert.IsTrue(adt.Patient.Suffix.IsNullOrEmpty());
            Assert.IsTrue(adt.Patient.Birthdate == new DateTime(1927, 03, 02));
            Assert.IsTrue(adt.Patient.Gender == Gender.Male);
            var address = adt.Patient.Addresses.First();
            Assert.IsTrue(address.Line1 == "3283 SOUTH RD");
            Assert.IsTrue(address.City == "WESTWOOD");
            Assert.IsTrue(address.StateOrProvince == "OH");
            Assert.IsTrue(address.PostalCode == "45248");
            var identifiers = adt.Patient.Identifiers.ToList()[0];
            Assert.IsTrue(identifiers.Type == "ssn" && identifiers.Value == "291221562");
            identifiers = adt.Patient.Identifiers.ToList()[1];
            Assert.IsTrue(identifiers.Type == "ssn4" && identifiers.Value == "1562");
            identifiers = adt.Patient.Identifiers.ToList()[2];
            Assert.IsTrue(identifiers.Type == "mbi" && identifiers.Value == "7JP4A74DV66");

            Assert.IsTrue(adt.BranchCode == "040");
            Assert.IsTrue(adt.Type == ADT.A01);



            //Assert.IsNull(adt.HchbPatient.PatientId);
            Assert.IsTrue(adt.HchbPatient.HchbPatientId == "598886");
            Assert.IsTrue(adt.HchbPatient.EpisodeId == "1114264");
            Assert.IsTrue(adt.HchbPatient.Status == "CURRENT");
            Assert.IsTrue(adt.HchbPatient.IcdCode == "8790");

            Assert.IsTrue(adt.RawFileName == "ADT/raw/ADT-A01-2023091508541804000059888601.txt");
            Assert.IsTrue(adt.JsonFileName == "ADT/json/ADT-A01-2023091508541804000059888601.json");

        }

        [Test]
        public void CreateAdtMessageWithHchbIdThenDeserializeThePatientIdShouldNullAndHchbPatientIdShouldValid() 
        {
            // Init
            string message = "{" +
                                "\"messageControlId\":\"2023091508541804000059888601\"," +
                                "\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                                "\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                                "\"patient\":" +
                                "  {" +
                                "   \"firstName\":\"RAYMOND\"," +
                                "   \"middleName\":\"A\"," +
                                "   \"lastName\":\"STERWERF\"," +
                                "   \"suffix\":\"\"," +
                                "   \"birthDate\":\"1927-03-02\"," +
                                "   \"gender\":\"M\"," +
                                "   \"addressLine1\":\"3283 SOUTH RD\"," +
                                "   \"addressLine2\":\"\"," +
                                "   \"city\":\"WESTWOOD\"," +
                                "   \"state\":\"OH\"," +
                                "   \"postalCode\":\"45248\"," +
                                "   \"identifiers\":" +
                                "    [" +
                                "       {" +
                                "         \"type\":\"ssn\"," +
                                "         \"value\":\"291221562\"" +
                                "       }," +
                                "       {" +
                                "         \"type\":\"ssn4\"," +
                                "         \"value\":\"1562\"" +
                                "       }," +
                                "       {" +
                                "         \"type\":\"mbi\"," +
                                "         \"value\":\"7JP4A74DV66\"" +
                                "       }" +
                                "   ]" +
                                "}," +
                                "\"type\":\"A01\"," +
                                "\"branchCode\":\"040\"," +
                                "\"patient_hchb\"" +
                                "   :{" +
                                "       \"externalId\":\"\"," +
                                "       \"patientId\":\"\"," +
                                "       \"HchbId\":\"598886\"," +
                                "       \"admissionId\":\"\"," +
                                "       \"episodeId\":\"1114264\"," +
                                "       \"status\":\"CURRENT\"," +
                                "       \"physicianNpi\":\"1518904879\"," +
                                "       \"physicianFirstName\":\"JOSEPH\"," +
                                "       \"physicianLastName\":\"SEIBERT\"," +
                                "       \"icd10code\":\"8790\"" +
                                "    }," +
                                "\"rawfilename\":\"ADT/raw/ADT-A01-2023091508541804000059888601.txt\"," +
                                "\"jsonfilename\":\"ADT/json/ADT-A01-2023091508541804000059888601.json\"}";

            // Act
            Adt adt = new Adt(message);

            // Assert
            Assert.IsNotNull(adt.HchbPatient.HchbPatientId == "598886");
        }

        [Test]
        public void CreateAdtMessageWithHchbIdAndPatientIdIncludedInHchbPatientWebThenDeserializeShoulExtractAllFieldsAsSubmitted() 
        {
            // Init
            string message = "{" +
                                "\"messageControlId\":\"2023102707222004200021935201\"," +
                                "\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                                "\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                                "\"patient\":" +
                                "   {" +
                                "       \"firstName\":\"EVELYN\"," +
                                "       \"middleName\":\"R\"," +
                                "       \"lastName\":\"COLLINS\"," +
                                "       \"suffix\":\"\"," +
                                "       \"birthDate\":\"1947-08-30\"," +
                                "       \"gender\":\"F\"," +
                                "       \"addressLine1\":\"337 WESLYN WAY\"," +
                                "       \"addressLine2\":\"\"," +
                                "       \"city\":\"NICHOLASVILLE\"," +
                                "       \"state\":\"KY\"," +
                                "       \"postalCode\":\"40356\"," +
                                "       \"identifiers\":" +
                                "           [" +
                                "               {" +
                                "                   \"type\":\"ssn\"," +
                                "                   \"value\":\"288687451\"" +
                                "               }," +
                                "               {" +
                                "                   \"type\":\"ssn4\"," +
                                "                   \"value\":\"7451\"" +
                                "               }," +
                                "               {" +
                                "                   \"type\":\"mbi\"," +
                                "                   \"value\":\"7FX4WY7HK85\"" +
                                "               }" +
                                "           ]" +
                                "   }," +
                                "\"type\":\"A08\"," +
                                "\"branchCode\":\"042\"," +
                                "\"patient_hchb\":" +
                                "   {" +
                                "       \"patientId\":\"219352\"," +
                                "       \"episodeId\":\"1124483\"," +
                                "       \"status\":\"CURRENT\"," +
                                "       \"hchbId\":\"219352\"," +
                                "       \"icd10code\":\"U07.1\"" +
                                "   }," +
                                "\"rawfilename\":\"ADT/raw/ADT-A08-2023102707222004200021935201.txt\"," +
                                "\"jsonfilename\":\"ADT/json/ADT-A08-2023102707222004200021935201.json\"" +
                              "}";

            // Act
            Adt adt = new Adt(message);

            // Assert
            Assert.IsTrue(adt.MessageControlId == "2023102707222004200021935201");
            Assert.IsTrue(adt.Patient.FirstName == "EVELYN");
            Assert.IsTrue(adt.Patient.MiddleName == "R");
            Assert.IsTrue(adt.Patient.LastName == "COLLINS");
            Assert.IsEmpty(adt.Patient.Suffix);
            Assert.IsTrue(adt.Patient.Birthdate == new DateTime(1947, 08, 30));
            Assert.IsTrue(adt.Patient.Gender == Gender.Female);
            var address = adt.Patient.Addresses.FirstOrDefault();
            Assert.IsTrue(address?.Line1 == "337 WESLYN WAY");
            Assert.IsTrue(address?.Line2 == string.Empty);
            Assert.IsTrue(address?.City == "NICHOLASVILLE");
            Assert.IsTrue(address?.StateOrProvince == "KY");
            var identifiers = adt.Patient.Identifiers.ToList();
            Assert.IsTrue(identifiers[0].Type == "ssn" && identifiers[0].Value == "288687451");
            Assert.IsTrue(identifiers[1].Type == "ssn4" && identifiers[1].Value == "7451");
            Assert.IsTrue(identifiers[2].Type == "mbi" && identifiers[2].Value == "7FX4WY7HK85");
            Assert.IsTrue(adt.Type == ADT.A08);
            Assert.IsTrue(adt.BranchCode == "042");
            Assert.IsTrue(adt.HchbPatient.HchbPatientId == "219352");
            Assert.IsTrue(adt.HchbPatient.EpisodeId == "1124483");
            Assert.IsTrue(adt.HchbPatient.Status == "CURRENT");
            Assert.IsTrue(adt.HchbPatient.IcdCode == "U07.1");
            Assert.IsTrue(adt.RawFileName == "ADT/raw/ADT-A08-2023102707222004200021935201.txt");
            Assert.IsTrue(adt.JsonFileName == "ADT/json/ADT-A08-2023102707222004200021935201.json");


        }

        [Test]
        public void CreateAdtMessageFromJsonStringWithHchbIdShouldSuccessfullyDeserializeAllfieldsOfHchbPatient()
        {
            // Init
            string message = "{" +
                "\"messageControlId\":\"2023091508541804000059888601\"," +
                "\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "\"patient\":" +
                "  {" +
                "   \"firstName\":\"RAYMOND\"," +
                "   \"middleName\":\"A\"," +
                "   \"lastName\":\"STERWERF\"," +
                "   \"suffix\":\"\"," +
                "   \"birthDate\":\"1927-03-02\"," +
                "   \"gender\":\"M\"," +
                "   \"addressLine1\":\"3283 SOUTH RD\"," +
                "   \"addressLine2\":\"\"," +
                "   \"city\":\"WESTWOOD\"," +
                "   \"state\":\"OH\"," +
                "   \"postalCode\":\"45248\"," +
                "   \"identifiers\":" +
                "    [ ]" +
                "}," +
                "\"type\":\"A01\"," +
                "\"branchCode\":\"040\"," +
                "\"patient_hchb\"" +
                "   :{" +
                "       \"externalId\":\"\"," +
                "       \"patientId\":\"598886\"," +
                "       \"admissionId\":\"\"," +
                "       \"episodeId\":\"1114264\"," +
                "       \"status\":\"CURRENT\"," +
                "       \"physicianNpi\":\"1518904879\"," +
                "       \"physicianFirstName\":\"JOSEPH\"," +
                "       \"physicianLastName\":\"SEIBERT\"," +
                "       \"icd10code\":\"8790\"" +
                "    }," +
                "\"rawfilename\":\"ADT/raw/ADT-A01-2023091508541804000059888601.txt\"," +
                "\"jsonfilename\":\"ADT/json/ADT-A01-2023091508541804000059888601.json\"}";

            // Act
            Adt adt = new Adt(message);

            // Assert

            //Assert.IsNull(adt.HchbPatient.PatientId);
            Assert.IsTrue(adt.HchbPatient.HchbPatientId == "598886");

            Assert.IsTrue(adt.HchbPatient.EpisodeId == "1114264");
            Assert.IsTrue(adt.HchbPatient.Status == "CURRENT");
            Assert.IsTrue(adt.HchbPatient.IcdCode == "8790");
        }
        
        [Test]
        public void CreateAdtMessageFromJsonStingWithoutPatientShouldNotFail() 
        {
            // Init
            string message = "{" +
                "\"messageControlId\":\"2023091508541804000059888601\"," +
                "\"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "\"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "\"type\":\"A01\"," +
                "\"branchCode\":\"040\"," +
                "\"patient_hchb\"" +
                "   :{" +
                "       \"externalId\":\"\"," +
                "       \"patientId\":\"598886\"," +
                "       \"admissionId\":\"\"," +
                "       \"episodeId\":\"1114264\"," +
                "       \"status\":\"CURRENT\"," +
                "       \"physicianNpi\":\"1518904879\"," +
                "       \"physicianFirstName\":\"JOSEPH\"," +
                "       \"physicianLastName\":\"SEIBERT\"," +
                "       \"icd10code\":\"8790\"" +
                "    }," +
                "\"rawfilename\":\"ADT/raw/ADT-A01-2023091508541804000059888601.txt\"," +
                "\"jsonfilename\":\"ADT/json/ADT-A01-2023091508541804000059888601.json\"}";

            // Act
            Adt adt = new Adt(message);

            // Assert
            Assert.IsNull(adt.Patient);
        }

        [Test]
        public void CreateMdmMessageFromJsonStringShouldDeserializeAllFields() 
        {
            // init
           string message = "{" +
                "               \"messageControlId\":\"2023102617281504000062859501\"," +
                "               \"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "               \"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "               \"type\":\"T02\"," +
                "               \"status\":\"CURRENT\"," +
                "               \"patient\":" +
                "                   {" +
                "                      \"firstName\":\"THELMA\"," +
                "                      \"middleName\":\"M\"," +
                "                      \"lastName\":\"MOCK\"," +
                "                      \"suffix\":\"\"," +
                "                      \"birthDate\":\"1939-11-23\"," +
                "                      \"gender\":\"F\"," +
                "                      \"addressLine1\":\"1082 ARBORWOOD COURT\"," +
                "                      \"addressLine2\":\"\"," +
                "                      \"city\":\"BATAVIA\"," +
                "                      \"state\":\"OH\"," +
                "                      \"postalCode\":\"45103\"," +
                "                      \"identifiers\":[]" +
                "                   }," +
                "               \"transaction\":" +
                "                   {" +
                "                       \"orderDate\":\"20231026135402\"," +
                "                       \"orderNumber\":\"4875706\"," +
                "                       \"filename\":\"MDM/pdf/040_2_4875706.pdf\"," +
                "                       \"admitDate\":\"20230929000000\"," +
                "                       \"admissionType\":\"NEW ADMISSION\"," +
                "                       \"observationId\":\"2\"," +
                "                       \"observationText\":\"POCU\"," +
                "                       \"orderType\":\"2\"," +
                "                       \"patientType\":\"HOME HEALTH\"," +
                "                       \"sendDate\":\"20231026172405\"," +
                "                       \"hchbId\":\"628595\"," +
                "                       \"episodeId\":\"1127144\"" +
                "                   }," +
                "               \"signer\":" +
                "                   {" +
                "                       \"npi\":\"1649944919\"," +
                "                       \"firstName\":\"PEGGY\"," +
                "                       \"lastName\":\"GILDENBLATT\"," +
                "                       \"branchCode\":\"040\"" +
                "                   }," +
                "               \"sender\":" +
                "                   {" +
                "                       \"npi\":\"\"," +
                "                       \"firstName\":\"\"," +
                "                       \"lastName\":\"\"," +
                "                       \"branchCode\":\"040\"" +
                "                   }," +
                "               \"rawfilename\":\"MDM/raw/MDM-T02-2023102617281504000062859501.txt\"," +
                "               \"jsonfilename\":\"MDM/json/MDM-T02-2023102617281504000062859501.json\"" +
                "}";

            // Act
            Mdm mdm = new Mdm(message);

            // Assert
            Assert.IsTrue(mdm.MessageControlId == "2023102617281504000062859501");
            Assert.IsTrue(mdm.Status == "CURRENT");
            
            Assert.IsTrue(mdm.Transaction.OrderDate == new DateTime(2023, 10, 26, 13, 54, 02));
            Assert.IsTrue(mdm.Transaction.OrderNumber == "4875706");
            Assert.IsTrue(mdm.Transaction.FileName == "MDM/pdf/040_2_4875706.pdf");
            Assert.IsTrue(mdm.Transaction.AdmitDate == new DateTime(2023, 09, 29));
            Assert.IsTrue(mdm.Transaction.AdmissionType == "NEW ADMISSION");
            Assert.IsTrue(mdm.Transaction.ObservationId == "2");
            Assert.IsTrue(mdm.Transaction.ObservationText == "POCU");
            Assert.IsTrue(mdm.Transaction.PatientType == "HOME HEALTH");
            Assert.IsTrue(mdm.Transaction.SendDate == new DateTime(2023, 10,26,17,24,05));
            Assert.IsTrue(mdm.Transaction.HchbPatientId == "628595");
            Assert.IsTrue(mdm.Transaction.EpisodeId == "1127144");

            Assert.IsTrue(mdm.Patient.FirstName == "THELMA");
            Assert.IsTrue(mdm.Patient.MiddleName == "M");
            Assert.IsTrue(mdm.Patient.LastName == "MOCK");
            Assert.IsTrue(mdm.Patient.Suffix == string.Empty );
            Assert.IsTrue(mdm.Patient.Birthdate == new DateTime(1939, 11, 23));
            Assert.IsTrue(mdm.Patient.Gender == Gender.Female);            
            Assert.IsTrue(mdm.Patient.AddressLine1 == "1082 ARBORWOOD COURT");
            Assert.IsTrue(mdm.Patient.AddressLine2 == string.Empty);
            Assert.IsTrue(mdm.Patient.City == "BATAVIA");
            Assert.IsTrue(mdm.Patient.StateOrProvince == "OH");
            Assert.IsTrue(mdm.Patient.PostalCode == "45103");
            Assert.IsEmpty(mdm.Patient.Ids);

            Assert.IsTrue(mdm.Signer.Npi == 1649944919);
            Assert.IsTrue(mdm.Signer.FirstName == "PEGGY");
            Assert.IsTrue(mdm.Signer.LastName == "GILDENBLATT");
            Assert.IsTrue(mdm.Signer.BranchCode == "040");

            Assert.IsTrue(mdm.Sender.Npi == 1891111911);
            Assert.IsTrue(mdm.Sender.FirstName == "HCHB");
            Assert.IsTrue(mdm.Sender.LastName == "User");
            Assert.IsTrue(mdm.Signer.BranchCode == "040");

            Assert.IsTrue(mdm.Rawfilename == "MDM/raw/MDM-T02-2023102617281504000062859501.txt");
            Assert.IsTrue(mdm.Jsonfilename == "MDM/json/MDM-T02-2023102617281504000062859501.json");



        }

        [Test]
        public void CreatMdmMessageThenSerializeShouldSuccess() 
        {
            //Init
            _ = "{" +
      "               \"messageControlId\":\"2023102617281504000062859501\"," +
      "               \"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
      "               \"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
      "               \"type\":\"T02\"," +
      "               \"status\":\"CURRENT\"," +
      "               \"patient\":" +
      "                   {" +
      "                      \"firstName\":\"THELMA\"," +
      "                      \"middleName\":\"M\"," +
      "                      \"lastName\":\"MOCK\"," +
      "                      \"suffix\":\"\"," +
      "                      \"birthDate\":\"1939-11-23\"," +
      "                      \"gender\":\"F\"," +
      "                      \"addressLine1\":\"1082 ARBORWOOD COURT\"," +
      "                      \"addressLine2\":\"\"," +
      "                      \"city\":\"BATAVIA\"," +
      "                      \"state\":\"OH\"," +
      "                      \"postalCode\":\"45103\"," +
      "                      \"identifiers\":[]" +
      "                   }," +
      "               \"transaction\":" +
      "                   {" +
      "                       \"orderDate\":\"20231026135402\"," +
      "                       \"orderNumber\":\"4875706\"," +
      "                       \"filename\":\"MDM/pdf/040_2_4875706.pdf\"," +
      "                       \"admitDate\":\"20230929000000\"," +
      "                       \"admissionType\":\"NEW ADMISSION\"," +
      "                       \"observationId\":\"2\"," +
      "                       \"observationText\":\"POCU\"," +
      "                       \"orderType\":\"2\"," +
      "                       \"patientType\":\"HOME HEALTH\"," +
      "                       \"sendDate\":\"20231026172405\"," +
      "                       \"hchbId\":\"628595\"," +
      "                       \"episodeId\":\"1127144\"" +
      "                   }," +
      "               \"signer\":" +
      "                   {" +
      "                       \"npi\":\"1649944919\"," +
      "                       \"firstName\":\"PEGGY\"," +
      "                       \"lastName\":\"GILDENBLATT\"," +
      "                       \"branchCode\":\"040\"" +
      "                   }," +
      "               \"sender\":" +
      "                   {" +
      "                       \"npi\":\"\"," +
      "                       \"firstName\":\"\"," +
      "                       \"lastName\":\"\"," +
      "                       \"branchCode\":\"040\"" +
      "                   }," +
      "               \"rawfilename\":\"MDM/raw/MDM-T02-2023102617281504000062859501.txt\"," +
      "               \"jsonfilename\":\"MDM/json/MDM-T02-2023102617281504000062859501.json\"" +
      "}";
            Mdm mdm = new Mdm();
            mdm.MessageControlId = "2023102617281504000062859501";
            mdm.Rawfilename = "MDM/raw/MDM-T02-2023102617281504000062859501.txt";
            mdm.Jsonfilename = "MDM/json/MDM-T02-2023102617281504000062859501.json";
            mdm.Status = "CURRENT";
            
            mdm.Patient = new()
            {
                FirstName = "THELMA",
                MiddleName = "M",
                LastName = "MOCK",
                Suffix = "",
                Birthdate = new DateTime(1939, 11, 23),
                Gender = Gender.Female,
                AddressLine1 = "1082 ARBORWOOD COURT",
                AddressLine2 = "",
                City = "BATAVIA",
                StateOrProvince = "OH",
                PostalCode = "45103"
            };
            mdm.Sender = new()
            {
                Npi = 0,
                FirstName = string.Empty,
                LastName = string.Empty,
                BranchCode = "040"
            };
            mdm.Signer = new()
            {
                Npi = 1649944919,
                FirstName = "PEGGY",
                LastName = "GILDENBLATT",
                BranchCode = "040"
            };
            mdm.Transaction = new()
            {
                OrderDate = DateTime.ParseExact("20231026135402", "yyyyMMddHHmmss", null),
                OrderNumber = "4875706",
                FileName = "MDM/pdf/040_2_4875706.pdf",
                AdmitDate = DateTime.ParseExact("20230929000000", "yyyyMMddHHmmss", null),
                AdmissionType = "NEW ADMISSION",
                ObservationId = "2",
                ObservationText = "POCU",
                PatientType = "HOME HEALTH",
                SendDate = DateTime.ParseExact("20231026172405", "yyyyMMddHHmmss", null),
                HchbPatientId = "628595",
                EpisodeId = "1127144"
            };

            // Act
            JsonConverter[] converters = new JsonConverter[] { new MdmConverter(), new PatientMatchingRequestConverter(), new TransactionConverter(), new SenderConverter(), new SignerConverter() };
            string json = JsonConvert.SerializeObject(mdm, converters);

            // Assert
            Assert.IsTrue(json.Contains("\"messageControlId\":\"2023102617281504000062859501\""));
            Assert.IsTrue(json.Contains("\"status\":\"CURRENT\""));
            Assert.IsTrue(json.Contains("\"firstName\":\"THELMA\""));
            Assert.IsTrue(json.Contains("\"middleName\":\"M\""));
            Assert.IsTrue(json.Contains("\"birthDate\":\"1939-11-23\""));
            Assert.IsTrue(json.Contains("\"gender\":\"F\""));
            Assert.IsTrue(json.Contains("\"addressLine1\":\"1082 ARBORWOOD COURT\""));
            Assert.IsTrue(json.Contains("\"city\":\"BATAVIA\""));
            Assert.IsTrue(json.Contains("\"state\":\"OH\""));
            Assert.IsTrue(json.Contains("\"postalCode\":\"45103\""));
            Assert.IsTrue(json.Contains("\"orderDate\":\"20231026135402\""));
            Assert.IsTrue(json.Contains("\"orderNumber\":\"4875706\""));
            Assert.IsTrue(json.Contains("\"filename\":\"MDM/pdf/040_2_4875706.pdf\""));
            Assert.IsTrue(json.Contains("\"admitDate\":\"20230929000000\""));
            Assert.IsTrue(json.Contains("\"admissionType\":\"NEW ADMISSION\""));
            Assert.IsTrue(json.Contains("\"observationId\":\"2\""));
            Assert.IsTrue(json.Contains("\"observationText\":\"POCU\""));            
            Assert.IsTrue(json.Contains("\"sendDate\":\"20231026172405\""));
            Assert.IsTrue(json.Contains("\"hchbId\":\"628595\""));
            Assert.IsTrue(json.Contains("\"episodeId\":\"1127144\""));
            Assert.IsTrue(json.Contains("\"npi\":1649944919"));
            Assert.IsTrue(json.Contains("\"firstName\":\"PEGGY\""));
            Assert.IsTrue(json.Contains("\"lastName\":\"GILDENBLATT\""));
            Assert.IsTrue(json.Contains("\"branchCode\":\"040\""));
            Assert.IsTrue(json.Contains("\"rawfilename\":\"MDM/raw/MDM-T02-2023102617281504000062859501.txt\""));
            Assert.IsTrue(json.Contains("\"jsonfilename\":\"MDM/json/MDM-T02-2023102617281504000062859501.json\""));
        }

        [Test]
        public void CreateHL7MessageLogFromMdmJsonStringShouldDeserializeAllFields() 
        {
            string json = "{" +
                "               \"messageControlId\":\"2023101018241204000050144501\"," +
                "               \"clientApplicationId\":\"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "               \"clientFacilityId\":\"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "               \"type\":\"T02\"," +
                "               \"status\":\"CURRENT\"," +
                "               \"patient\":" +
                "                   {" +
                "                       \"firstName\":\"JACQUELINE\"," +
                "                       \"middleName\":\"\"," +
                "                       \"lastName\":\"MURPHY\"," +
                "                       \"suffix\":\"\"," +
                "                       \"birthDate\":\"1941-03-23\"," +
                "                       \"gender\":\"F\"," +
                "                       \"addressLine1\":\"7445 RED COAT DRIVE\"," +
                "                       \"addressLine2\":\"\"," +
                "                       \"city\":\"FAIRFIELD\"," +
                "                       \"state\":\"OH\"," +
                "                       \"postalCode\":\"45011\"," +
                "                       \"identifiers\":[]" +
                "                   }," +
                "               \"transaction\":" +
                "                   {" +
                "                       \"orderDate\":\"20230915151107\"," +
                "                       \"orderNumber\":\"4803057\"," +
                "                       \"filename\":\"MDM/pdf/040_1_4803057.pdf\"," +
                "                       \"admitDate\":\"20230918000000\"," +
                "                       \"admissionType\":\"RECERTIFICATION\"," +
                "                       \"observationId\":\"1\"," +
                "                       \"observationText\":\"485\"," +
                "                       \"orderType\":\"1\"," +
                "                       \"patientType\":\"HOME HEALTH\"," +
                "                       \"sendDate\":\"20231010181922\"," +
                "                       \"hchbId\":\"501445\"," +
                "                       \"episodeId\":\"1118531\"" +
                "                   }," +
                "               \"signer\":" +
                "                   {" +
                "                       \"npi\":\"1447621941\"," +
                "                       \"firstName\":\"TYRA\"," +
                "                       \"lastName\":\"MARTINEAR\"," +
                "                       \"branchCode\":\"040\"" +
                "                   }," +
                "               \"sender\":" +
                "                   {" +
                "                       \"npi\":\"\"," +
                "                       \"firstName\":\"\"," +
                "                       \"lastName\":\"\"," +
                "                       \"branchCode\":\"040\"" +
                "                   }," +
                "               \"rawfilename\":\"MDM/raw/MDM-T02-2023101018241204000050144501.txt\"," +
                "               \"jsonfilename\":\"MDM/json/MDM-T02-2023101018241204000050144501.json\"" +
                "}";

            HL7MessageLog hl7MessageLog = new HL7MessageLog(json, "MDM");
            Assert.IsTrue(hl7MessageLog.MessageControlId == "2023101018241204000050144501");
            Assert.IsTrue(hl7MessageLog.Type == "MDM");
            Assert.AreEqual("T02", hl7MessageLog.SubType);
            Assert.AreEqual(json.Substring(0, 1000), hl7MessageLog.Message);
            Assert.IsTrue(hl7MessageLog.RawMessageFile == "MDM/raw/MDM-T02-2023101018241204000050144501.txt");
            Assert.IsTrue(hl7MessageLog.JsonMessageFile == "MDM/json/MDM-T02-2023101018241204000050144501.json");
            Assert.IsTrue(hl7MessageLog.IsProcessed == false);
            Assert.IsTrue(hl7MessageLog.HchbPatientId == "501445");
            Assert.IsTrue(hl7MessageLog.EpisodeId == "1118531");
            Assert.IsTrue(hl7MessageLog.Reason == null);
            Assert.IsTrue(hl7MessageLog.ReceivedDate != null && hl7MessageLog.ReceivedDate.Value.Date == DateTime.Now.Date);
            Assert.IsTrue(hl7MessageLog.ProcessedDate == null);            

        }

        [Test]
        public void CreateHL7MessageLogFromAdtJsonStringShouldDeserializeAllFields()
        {
            string json = "{" +
                "           \r\n \"messageControlId\": \"2023103002032304100006287201\"," +
                "           \r\n \"clientApplicationId\": \"ugyf6LspQQ6Zk/XIcd7CpQ==\"," +
                "           \r\n \"clientFacilityId\": \"PJ8VPDT8F53SUD52JMGS96401K\"," +
                "           \r\n \"patient\": " +
                "                   {" +
                "                       \r\n \"firstName\": \"KATHRYN\"," +
                "                       \r\n \"middleName\": \"A\"," +
                "                       \r\n \"lastName\": \"KINDRED\"," +
                "                       \r\n \"suffix\": \"\"," +
                "                       \r\n \"birthDate\": \"1931-01-31\"," +
                "                       \r\n \"gender\": \"F\"," +
                "                       \r\n \"addressLine1\": \"137 RAINTREE DR\"," +
                "                       \r\n \"addressLine2\": \"\"," +
                "                       \r\n \"city\": \"FLORENCE\"," +
                "                       \r\n \"state\": \"KY\"," +
                "                       \r\n \"postalCode\": \"41042\"," +
                "                       \r\n \"identifiers\": " +
                "                                [" +
                "                                   \r\n {" +
                "                                            \r\n  \"type\": \"ssn\"," +
                "                                            \r\n  \"value\": \"290243809\"" +
                "                                   \r\n }," +
                "                                   \r\n {" +
                "                                             \r\n  \"type\": \"ssn4\"," +
                "                                             \r\n  \"value\": \"3809\"" +
                "                                   \r\n }," +
                "                                   \r\n {" +
                "                                             \r\n  \"type\": \"mbi\"," +
                "                                             \r\n  \"value\": \"1PH6F21WD57\"" +
                "                                   \r\n }" +
                "                        \r\n ]" +
                "           \r\n    }," +
                "           \r\n \"type\": \"A04\"," +
                "           \r\n \"branchCode\": \"041\"," +
                "           \r\n \"patient_hchb\": " +
                "                       {" +
                "                           \r\n \"patientId\": \"62872\"," +
                "                           \r\n \"episodeId\": \"1135199\"," +
                "                           \r\n \"status\": \"PENDING\"," +
                "                           \r\n \"hchbId\": \"62872\"," +
                "                           \r\n \"icd10code\": \"E11.51\"\r\n    }," +
                "                           \r\n \"rawfilename\": \"ADT/raw/ADT-A04-2023103002032304100006287201.txt\"," +
                "                           \r\n \"jsonfilename\": \"ADT/json/ADT-A04-2023103002032304100006287201.json" +
                "   \"\r\n}";

            HL7MessageLog hl7MessageLog = new HL7MessageLog(json, "ADT");
            Assert.IsTrue(hl7MessageLog.MessageControlId == "2023103002032304100006287201");
            Assert.IsTrue(hl7MessageLog.Type == "ADT");
            Assert.IsTrue(hl7MessageLog.SubType == "A04");
            Assert.AreEqual( json.Substring(0, 1000), 
                hl7MessageLog.Message);
            Assert.IsTrue(hl7MessageLog.RawMessageFile == "ADT/raw/ADT-A04-2023103002032304100006287201.txt");
            Assert.IsTrue(hl7MessageLog.JsonMessageFile == "ADT/json/ADT-A04-2023103002032304100006287201.json");
            Assert.IsTrue(hl7MessageLog.IsProcessed == false);
            Assert.IsTrue(hl7MessageLog.HchbPatientId == "62872");
            Assert.IsTrue(hl7MessageLog.EpisodeId == "1135199");
            Assert.IsTrue(hl7MessageLog.Reason == null);
            Assert.IsTrue(hl7MessageLog.ReceivedDate != null && hl7MessageLog.ReceivedDate.Value.Date == DateTime.Now.Date);
            Assert.IsTrue(hl7MessageLog.ProcessedDate == null);

        }

        [Test]
        public void SerializeAnOruMessageShouldSuccess()
        {
            // Init
            Oru oru = new Oru()
            {
                FirstName = "Theresa",
                LastName = "Dean",
                Gender = "F",
                BirthDate = "19570313",
                PatientId = "423128",
                EpisodeId = "1117282",
                OrderNumber = "4882783",
                ObservationId = "6",
                ObservationText = "PO",
                RequestedDateTime = "20231031102940",
                RejectReason = "patient not found",
                ResultDate = "20231101195320",
                ResultStatus = "F"                
            };

            // Act
            string json = JsonConvert.SerializeObject(oru, new JsonConverter[] { new OruConverter() });

            // Assert
            Assert.IsTrue(json.Contains("\"firstName\":\"Theresa\""));
            Assert.IsTrue(json.Contains("\"lastName\":\"Dean\""));
            Assert.IsTrue(json.Contains("\"gender\":\"F\""));
            Assert.IsTrue(json.Contains("\"birthDate\":\"19570313\""));
            Assert.IsTrue(json.Contains("\"patientId\":\"423128\""));
            Assert.IsTrue(json.Contains("\"episodeId\":\"1117282\""));
            Assert.IsTrue(json.Contains("\"orderNumber\":\"4882783\""));
            Assert.IsTrue(json.Contains("\"observationId\":\"6\""));
            Assert.IsTrue(json.Contains("\"observationText\":\"PO\""));
            Assert.IsTrue(json.Contains("\"requestedDateTime\":\"20231031102940\""));
            Assert.IsTrue(json.Contains("\"rejectReason\":\"patient not found\""));
            Assert.IsTrue(json.Contains("\"resultDate\":\"20231101195320\""));
            Assert.IsTrue(json.Contains("\"resultStatus\":\"F\""));

        }

        [Test]
        public void DeSerializeAnOruMessageShouldSuccess()
        {
            // Init
            string json = "{\r\n  \"requestId\" : 171087210,\r\n  \"firstName\" : \"Larry\",\r\n  \"lastName\" : \"Minor\",\r\n  \"gender\" : \"M\",\r\n  \"birthDate\" : \"20001129\",\r\n  \"patientId\" : \"303460\",\r\n  \"episodeId\" : \"1115533\",\r\n  \"orderNumber\" : \"4792663\",\r\n  \"observationId\" : \"1\",\r\n  \"observationText\" : \"485\",\r\n  \"requestedDateTime\" : \"20230911131000\",\r\n  \"rejectReason\" : \"\",\r\n  \"resultDate\" : \"20231019115223\",\r\n  \"resultStatus\" : \"F\"\r\n}";

            // Act
            Oru? oru = JsonConvert.DeserializeObject<Oru>(json, new JsonConverter[] { new OruConverter() });

            // Assert
            Assert.IsNotNull(oru);
            Assert.IsTrue(oru?.FirstName == "Larry");
            Assert.IsTrue(oru?.LastName == "Minor");
            Assert.IsTrue(oru?.Gender == "M");
            Assert.IsTrue(oru?.BirthDate == "20001129");
            Assert.IsTrue(oru?.PatientId == "303460");
            Assert.IsTrue(oru?.EpisodeId == "1115533");
            Assert.IsTrue(oru?.OrderNumber == "4792663");
            Assert.IsTrue(oru?.ObservationId == "1");
            Assert.IsTrue(oru?.ObservationText == "485");
            Assert.IsTrue(oru?.RequestedDateTime == "20230911131000");
            Assert.IsTrue(oru?.ResultDate == "20231019115223");
            Assert.IsTrue(oru?.ResultStatus == "F");
        }

        [Test]
        public void SerializeAnOruStreamShouldSuccess()
        {
            // Init
            OruStream oru = new OruStream()
            {
                PatientId = 423128,
                RequestId = 1001,
                FirstName = "Theresa",
                LastName = "Dean",
                Gender = "F",
                BirthDate = "19570313",
                RejectReason = "patient not found",
                ResultDate = "20231101195320",
                ResultStatus = "F"
            };

            // Act
            string json = JsonConvert.SerializeObject(oru, new JsonConverter[] { new OruStreamConverter() });

            // Assert
            Assert.IsTrue(json.Contains("\"patientId\":423128"));
            Assert.IsTrue(json.Contains("\"requestId\":1001"));
            Assert.IsTrue(json.Contains("\"firstName\":\"Theresa\""));
            Assert.IsTrue(json.Contains("\"lastName\":\"Dean\""));
            Assert.IsTrue(json.Contains("\"gender\":\"F\""));
            Assert.IsTrue(json.Contains("\"birthDate\":\"19570313\""));
            Assert.IsTrue(json.Contains("\"rejectReason\":\"patient not found\""));
            Assert.IsTrue(json.Contains("\"resultDate\":\"20231101195320\""));
            Assert.IsTrue(json.Contains("\"resultStatus\":\"F\""));

        }

    }
}
