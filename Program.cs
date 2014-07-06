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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log.Info("FillHotelGeoLocations started");

            const int englandLocationId = 6269131;

            using (SelectedHotelsEntities db = new SelectedHotelsEntities())
            {
                var query = db.Products.OfType<Hotel>().Where(h => h.GeoLocationId != null);
                var hotels = query.ToList();
                Console.WriteLine(hotels.Count);
                foreach (Hotel hotel in hotels)
                {
                    Console.WriteLine(hotels.IndexOf(hotel));

                    int currentGeoLocationId = hotel.GeoLocationId.Value;

                    var geoName = db.GeoNames.Find(currentGeoLocationId);
                    if (geoName == null)
                    {
                        log.Info(String.Format("GeoName {0} is not found for hotel {1}.", currentGeoLocationId, hotel.Id));
                        continue;
                    }

                    AddHotelGeoLocation(hotel, currentGeoLocationId, db);
                    var hierarchy = db.Hierarchies.SingleOrDefault(h => h.ChildId == currentGeoLocationId);
                    if (hierarchy != null)
                    {
                        if (!String.IsNullOrEmpty(hierarchy.Lineage))
                        {
                            string[] parents = hierarchy.Lineage.Split('/');
                            foreach (string parent in parents)
                            {
                                if (Common.Utils.IsNumeric(parent))
                                {
                                    currentGeoLocationId = int.Parse(parent);
                                    geoName = db.GeoNames.Find(currentGeoLocationId);
                                    if (geoName == null)
                                    {
                                        log.Info(String.Format("GeoName {0} is not found for hotel {1}.", currentGeoLocationId, hotel.Id));
                                        continue;
                                    }
                                    AddHotelGeoLocation(hotel, currentGeoLocationId, db);
                                }
                                else
                                {
                                    log.Info(String.Format("Parent {0} is not numberic.", parent));
                                }
                            }
                        }
                        else
                        {
                            log.Info(String.Format("Linear is empty for hierarchy {0}.", hierarchy.Id));
                        }
                    }
                    else
                    {
                        log.Info(String.Format("Hierarchy {0} is not found for hotel {1}.", currentGeoLocationId, hotel.Id));
                    }
                    if (hotels.IndexOf(hotel)%10 == 0)
                    {
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
            }
        }

        private static void AddHotelGeoLocation(Hotel hotel, int currentGeoLocationId, SelectedHotelsEntities db)
        {
            HotelGeoLocation hotelGeoLocation = new HotelGeoLocation();
            hotelGeoLocation.HotelId = hotel.Id;
            hotelGeoLocation.LocationId = currentGeoLocationId;
            hotelGeoLocation.HotelTypeId = hotel.HotelTypeId;
            db.HotelGeoLocations.Add(hotelGeoLocation);
        }
    }
}
