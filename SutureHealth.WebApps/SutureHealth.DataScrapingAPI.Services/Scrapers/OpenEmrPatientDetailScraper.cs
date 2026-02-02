
using AngleSharp.Html.Dom;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SutureHealth.DataScraping.Scrapers
{
    internal class OpenEmrPatientDetailScraper : IScraper
    {
        public IHtmlDocument HtmlDoc { get; set; }
        public OpenEmrPatientDetailScraper(IHtmlDocument htmlDoc)
        {
           HtmlDoc = htmlDoc;
        }

        public ScrapedPatientDetailHistory Scrape()
        {                        
            var document = HtmlDoc.GetElementsByName("pat");

            if (!document.IsNullOrEmpty())
            {
                //patient
                //var Id = Guid.NewGuid();
                var patientFirstName = HtmlDoc.QuerySelector("#text_fname")?.TextContent;
                var patientMiddleName = HtmlDoc.QuerySelector("#text_mname")?.TextContent;
                var patientLastName = HtmlDoc.QuerySelector("#text_lname")?.TextContent;
                var patientExternalId = HtmlDoc.QuerySelector("#text_pubpid")?.TextContent;
                var patientDOB = DateTime.Parse(HtmlDoc.QuerySelector("#text_DOB")?.TextContent);
                var patientGender = HtmlDoc.QuerySelector("#text_sex")?.TextContent;
                var patientMaritalStatus = HtmlDoc.QuerySelector("#text_status")?.TextContent;
                var patientSexualOrientation = HtmlDoc.QuerySelector("#text_sexual_orientation")?.TextContent;
                var attendedPhysician = HtmlDoc.QuerySelector("#text_providerID")?.TextContent;

                //patient address and contact
                var patientPostalCode = HtmlDoc.QuerySelector("#text_postal_code")?.TextContent;
                var patientAddress = HtmlDoc.QuerySelector("#text_street")?.TextContent + " " +
                                     HtmlDoc.QuerySelector("#text_street_line_2")?.TextContent;
                var patientCity = HtmlDoc.QuerySelector("#text_city")?.TextContent;
                var patientCountry = HtmlDoc.QuerySelector("#text_country_code")?.TextContent;
                var patientHomePhone = HtmlDoc.QuerySelector("#text_phone_home")?.TextContent;
                var patientWorkPhone = HtmlDoc.QuerySelector("#text_phone_biz")?.TextContent;
                var patientMobilePhone = HtmlDoc.QuerySelector("#text_phone_cell")?.TextContent;
                var patientEmail = HtmlDoc.QuerySelector("#text_email")?.TextContent;
                var patientTrustedEmail = HtmlDoc.QuerySelector("#text_email_direct")?.TextContent;

                //patient billing
                var patientBillings = HtmlDoc.QuerySelector("#billing_ps_expand")?.GetElementsByClassName("col");
                var patientBalance = patientBillings[0]?.TextContent;
                var insuranceBalance = patientBillings[1]?.TextContent;
                var totalBalance = patientBillings[2]?.TextContent;

                //patient employer
                var patientOccupation = HtmlDoc.QuerySelector("#text_occupation")?.TextContent;
                var patientEmployer = HtmlDoc.QuerySelector("#text_em_name")?.TextContent;

                //patient stats
                var patientLang = HtmlDoc.QuerySelector("#text_language")?.TextContent;
                var patientEtnicity = HtmlDoc.QuerySelector("#text_ethnicity")?.TextContent;
                var patientRace = HtmlDoc.QuerySelector("#text_race")?.TextContent;
                var patientFamilySize = HtmlDoc.QuerySelector("#text_family_size")?.TextContent;
                var patientMonthlyIncome = HtmlDoc.QuerySelector("#text_monthly_income")?.TextContent;
                var patientReligion = HtmlDoc.QuerySelector("#text_religion")?.TextContent;
                var patientDeceaseDate = HtmlDoc.QuerySelector("#text_deceased_date")?.TextContent;
                var patientDeceaseReason = HtmlDoc.QuerySelector("#text_deceased_reason")?.TextContent;

                //patient diagnoses
                var medicalProblems = Regex.Replace(HtmlDoc.QuerySelector("#medical_problem_ps_expand")?.TextContent?.Replace("\n", ""), @"\s{2,}", ";")?.Remove(0,1);
                var allergies = HtmlDoc.GetElementById("allergy_ps_expand").GetElementsByClassName("list-group-item p-1");
                var medications = Regex.Replace(HtmlDoc.QuerySelector("#medication_ps_expand")?.TextContent?.Replace("\n", ""), @"\s{2,}", ";")?.Remove(0, 1);
                var immunizations = HtmlDoc.QuerySelector("#immunizations_ps_expand")?.GetElementsByTagName("a");
                var prescriptions = HtmlDoc.GetElementById("prescriptions_ps_expand");
                var vitals = HtmlDoc.QuerySelector("#labdata_ps_expand")?.TextContent.Trim();
                var labs = HtmlDoc.QuerySelector("#vitals_ps_expand")?.TextContent.Trim();
                
                //patient contact assignments
                List<ContactHistory> patientContacts = new List<ContactHistory>(5);
                if (!string.IsNullOrEmpty(patientMobilePhone))
                {
                    //Contact mobilePhone = new Contact(Id, patientMobilePhone, ContactType.MobilePhone, 1);
                    ContactHistory mobilePhone = new ContactHistory();
                    mobilePhone.ContactText = patientMobilePhone;
                    mobilePhone.ContactType = ContactType.MobilePhone;
                    mobilePhone.PreferenceOrder = 1;
                    patientContacts.Add(mobilePhone);
                }
                if (!string.IsNullOrEmpty(patientHomePhone))
                {
                    //Contact homePhone = new Contact(Id, patientHomePhone, ContactType.HomePhone, 2);
                    ContactHistory homePhone = new ContactHistory();
                    homePhone.ContactText = patientHomePhone;
                    homePhone.ContactType = ContactType.HomePhone;
                    homePhone.PreferenceOrder = 2;
                    patientContacts.Add(homePhone);
                }
                if (!string.IsNullOrEmpty(patientWorkPhone))
                {
                    //Contact workPhone = new Contact(Id, patientWorkPhone, ContactType.WorkPhone, 3);
                    ContactHistory workPhone = new ContactHistory();
                    workPhone.ContactText = patientHomePhone;
                    workPhone.ContactType = ContactType.WorkPhone;
                    workPhone.PreferenceOrder = 3;
                    patientContacts.Add(workPhone);
                }
                if (!string.IsNullOrEmpty(patientTrustedEmail))
                {
                    //Contact trustedEmail = new Contact(Id, patientTrustedEmail, ContactType.Email, 4);
                    ContactHistory trustedEmail = new ContactHistory();
                    trustedEmail.ContactText = patientTrustedEmail;
                    trustedEmail.ContactType = ContactType.Email;
                    trustedEmail.PreferenceOrder = 4;
                    patientContacts.Add(trustedEmail);
                }
                if (!string.IsNullOrEmpty(patientEmail))
                {
                    //Contact email = new Contact(Id, patientEmail, ContactType.Email, 5);
                    ContactHistory email = new ContactHistory();
                    email.ContactText = patientEmail;
                    email.ContactType = ContactType.Email;
                    email.PreferenceOrder = 4;
                    patientContacts.Add(email);
                }

                //patient condition assignment
                List<ConditionHistory> patientConditions = new List<ConditionHistory>();
                if (!string.IsNullOrEmpty(medicalProblems))
                {
                    var conditions = medicalProblems.Remove(medicalProblems.Length-1, 1).Split(';');
                    foreach (var condition in conditions)
                    {
                        ConditionHistory conditionObj = new ConditionHistory();
                        conditionObj.Diagnosis = condition;
                        patientConditions.Add(conditionObj);    
                        //patientConditions.Add(new Condition(Id, condition));
                    }
                }

                //patient allergies assignment
                List<AllergyHistory> patientAllergies = new List<AllergyHistory>();
                if (!allergies.IsNullOrEmpty())
                {
                    foreach (var allergy in allergies)
                    {
                        AllergyHistory allergyObj = new AllergyHistory();
                        //allergyObj.PatientId = Id;
                        var nameAndReaction = allergy.GetElementsByClassName("flex-fill")[0]?.TextContent.Remove(0, 2);
                        allergyObj.Name = nameAndReaction?.Substring(0, nameAndReaction.IndexOf("\n")).Trim();
                        allergyObj.Reaction = allergy.GetElementsByTagName("small")[0]?.TextContent;
                        allergyObj.Severity = allergy.GetElementsByClassName("text-right")[0]?.TextContent;
                        //allergyObj.Id = Guid.NewGuid();
                        patientAllergies.Add(allergyObj);
                    }
                }

                //patient observation assignment
                List<ObservationHistory> patientObservations = new List<ObservationHistory>();
                if (!string.IsNullOrEmpty(vitals) || !string.IsNullOrEmpty(labs))
                {
                    ObservationHistory observation = new ObservationHistory();
                    //observation.PatientId = Id;
                    //observation.Id = Guid.NewGuid();
                    observation.Labs = labs;
                    observation.Vitals = vitals;

                    patientObservations.Add(observation);
                }

                //patient medication assignment
                List<MedicationHistory> patientMedications = new List<MedicationHistory>();
                if (!string.IsNullOrEmpty(medications))
                {
                    var meds = medications.Remove(medications.Length - 1, 1).Split(';');
                    foreach (var medication in meds)
                    {
                        MedicationHistory med = new MedicationHistory();
                        //med.PatientId = Id;
                        med.Name = medication;
                        patientMedications.Add(med);
                    }
                }

                //patient immunization assignment
                List<ImmunizationHistory> patientImmunizations = new List<ImmunizationHistory>();
                if (!immunizations.IsNullOrEmpty())
                {                    
                    foreach (var immunization in immunizations)
                    {
                        ImmunizationHistory immunizationObj = new ImmunizationHistory();
                        //immunizationObj.PatientId = Id;

                        string immunizationText = immunization.TextContent;
                        if ((immunizationText.IndexOf("-") != -1) && (immunizationText.IndexOf("-") != 0))
                        {
                            immunizationObj.AdministrationDate = DateTime.Parse(immunizationText.Substring(0,immunizationText.LastIndexOf("-")));
                            immunizationObj.Name = immunizationText.Substring(immunizationText.LastIndexOf("-") + 1);
                        }
                        else
                        {
                            immunizationObj.Name = immunization.TextContent;
                        }
                        patientImmunizations.Add(immunizationObj);
                    }
                }

                //prescription assignment
                List<PrescriptionHistory> patientPrescriptions = new List<PrescriptionHistory>();
                if (prescriptions.TextContent.Trim() != "None")
                {
                    var prescps = prescriptions?.GetElementsByTagName("tbody")[0]?.GetElementsByTagName("tr");
                    foreach (var prescription in prescps)
                    {
                        var prescriptionRow = prescription.GetElementsByTagName("td");

                        PrescriptionHistory prescriptionObj = new PrescriptionHistory();
                        //prescriptionObj.PatientId = Id;
                        prescriptionObj.DrugName = prescriptionRow[0].TextContent.Replace("&nbsp;", "");
                        prescriptionObj.Details = prescriptionRow[1].TextContent.Replace("&nbsp;", " ").Replace("\n", "").Trim();
                        prescriptionObj.Quantity = prescriptionRow[2].TextContent;
                        prescriptionObj.Refills = prescriptionRow[3].TextContent;
                        prescriptionObj.FillDate = DateTime.Parse(prescriptionRow[4].TextContent);

                        patientPrescriptions.Add(prescriptionObj);
                    }
                }                

                //Procedure assignment
                List <ProcedureHistory> patientProcedures = new List<ProcedureHistory>();
                if (patientAddress == " ")
                {
                    patientAddress = "";
                }
                

                ScrapedPatientDetailHistory scrapedPatientDetailModel = new ScrapedPatientDetailHistory(
                                                                                            patientExternalId,
                                                                                            patientFirstName,
                                                                                            patientMiddleName,
                                                                                            patientLastName,
                                                                                            patientDOB,
                                                                                            patientGender,
                                                                                            patientMaritalStatus,
                                                                                            patientSexualOrientation,
                                                                                            attendedPhysician,
                                                                                            patientAddress,
                                                                                            patientCity,
                                                                                            patientCountry,
                                                                                            patientPostalCode,
                                                                                            patientBalance,
                                                                                            insuranceBalance,
                                                                                            totalBalance,
                                                                                            patientOccupation,
                                                                                            patientEmployer,
                                                                                            patientLang,
                                                                                            patientEtnicity,
                                                                                            patientRace,
                                                                                            patientFamilySize,
                                                                                            patientMonthlyIncome,
                                                                                            patientReligion,
                                                                                            patientDeceaseDate,
                                                                                            patientDeceaseReason,
                                                                                            "",                                                                                                    
                                                                                            patientObservations,
                                                                                            patientContacts,
                                                                                            patientConditions,
                                                                                            patientAllergies,
                                                                                            patientMedications, 
                                                                                            patientImmunizations,
                                                                                            patientPrescriptions,
                                                                                            patientProcedures);

                return scrapedPatientDetailModel;
            }
            else
            {
                return new ScrapedPatientDetailHistory();

            }
        }
    }
}
