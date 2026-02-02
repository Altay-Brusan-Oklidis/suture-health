using SutureHealth.Hchb.Services.Testing.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Address
{
    public class AddressBuilder
    {
        AddressType addressInstance;

        public AddressBuilder()
        {
            addressInstance = new AddressType();
        }
        public AddressType Build([Optional] string streetName,
                          [Optional] string city,
                          [Optional] string state,
                          [Optional] FascilityType? addressType,
                          [Optional] string facilityName,
                          [Optional] string facilityType,
                          [Optional] string facilityId,
                          [Optional] string zip)
        {

            string currentDirectory = Directory.GetCurrentDirectory();

            // create a line
            if (string.IsNullOrEmpty(streetName))
            {
                var streetAddressDataSetPath = Path.Combine(currentDirectory, "Data", "StreetAddressDataSet.txt");
                try
                {
                    var streetLines = File.ReadAllLines(streetAddressDataSetPath);
                    int count = streetLines.Count();
                    Random rnd = new Random();
                    var itemIndex = rnd.Next(0, count - 1);
                    addressInstance.StreetAddress = streetLines[itemIndex];
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                addressInstance.StreetAddress = streetName;
            }

            // create a city
            if (string.IsNullOrEmpty(city))
            {
                var cityDatasetPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "CityDataSet.txt");
                try
                {
                    var cityLines = File.ReadAllLines(cityDatasetPath.ToString());
                    Random rnd = new Random();
                    var count = cityLines.Count();
                    var itemIndex = rnd.Next(0, count - 1);
                    addressInstance.City = cityLines[itemIndex];
                }
                catch (Exception)
                {

                    throw;
                }

            }
            else
            {
                addressInstance.City = city;
            }

            // HCHB ignores country name
            /*
            //var countryDatasetPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "CountryDataSet.txt");
            //countryLines = File.ReadAllLines(countryDatasetPath.ToString());
            //count = countryLines.Count();            
            //itemIndex= rnd.Next(0, count - 1);
            //addressInstance. = countryLines[itemIndex].Substring(0, 3);
            */

            addressInstance.FacilityName = string.IsNullOrEmpty(facilityName) ? Utilities.GetRandomString(5) : facilityName;

            if (string.IsNullOrEmpty(facilityName))
            {
                var stateDatasetPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "StateDataSet.txt");
                try
                {
                    var stateLines = File.ReadAllLines(stateDatasetPath.ToString());
                    Random rnd = new Random();
                    var count = stateLines.Count();
                    var itemIndex = rnd.Next(0, count);
                    addressInstance.State = stateLines[itemIndex];

                }
                catch (Exception)
                {

                    throw;
                }

            }
            else
            {
                addressInstance.State = state;
            }


            if (addressType.HasValue)
            {
                addressInstance.FacilityType = addressType.Value;
            }
            else
            {
                addressInstance.FacilityType = FascilityType.P;
            }

            // HCHB: open for interpretation, no table exists.
            addressInstance.FacilityId = string.IsNullOrEmpty(facilityId) ? Utilities.GetRandomDecimalString(3) : facilityId;

            if (string.IsNullOrEmpty(zip))
            {
                var zipLinesPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ZipDataSet.txt");
                try
                {
                    var zipLines = File.ReadAllLines(zipLinesPath.ToString());
                    Random rnd = new Random();
                    var count = zipLines.Count();
                    var itemIndex = rnd.Next(0, count - 1);
                    addressInstance.ZipCode = zipLines[itemIndex];

                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                addressInstance.ZipCode = zip;
            }
            return addressInstance;
        }
        public AddressType GetAddress()
        {
            return addressInstance;
        }    

    }
}
