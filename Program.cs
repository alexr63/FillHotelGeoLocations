using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SelectedHotelsModel;

namespace FillHotelGeoLocations
{
    class Program
    {
        static void Main(string[] args)
        {
            const int englandLocationId = 6269131;

            using (SelectedHotelsEntities db = new SelectedHotelsEntities())
            {
                var hotels = db.Products.OfType<Hotel>().Where(h => h.GeoLocationId != null);
                foreach (Hotel hotel in hotels.ToList())
                {
                    int currentGeoLocationId = hotel.GeoLocationId.Value;
                    while (currentGeoLocationId != englandLocationId)
                    {
                        var geoName = db.GeoNames.Find(currentGeoLocationId);
                        if (geoName == null)
                        {
                            break;
                        }

                        HotelGeoLocation hotelGeoLocation = new HotelGeoLocation();
                        hotelGeoLocation.HotelId = hotel.Id;
                        hotelGeoLocation.LocationId = currentGeoLocationId;
                        hotelGeoLocation.HotelTypeId = hotel.HotelTypeId;
                        db.HotelGeoLocations.Add(hotelGeoLocation);

                        var hierarchy = db.Hierarchies.SingleOrDefault(h => h.ChildId == currentGeoLocationId);
                        if (hierarchy != null)
                        {
                            currentGeoLocationId = hierarchy.ParentId;
                        }
                        else
                        {
                            currentGeoLocationId = englandLocationId;
                        }
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}
