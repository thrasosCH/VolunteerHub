namespace VolunteerHub.Data
{
    public static class LocationData
    {
        public static Dictionary<string, List<string>> CountriesAndCities =
            new Dictionary<string, List<string>>
            {
                {
                    "Greece",
                    new List<string>
                    {
                        "Athens",
                        "Thessaloniki",
                        "Patras",
                        "Heraklion",
                        "Larissa",
                        "Volos",
                        "Ioannina",
                        "Kavala",
                        "Kalamata",
                        "Serres",
                        "Katerini",
                        "Kilkis"
                    }
                },

                {
                    "Switzerland",
                    new List<string>
                    {
                        "Zurich",
                        "Geneva",
                        "Basel",
                        "Bern",
                        "Lausanne",
                        "Lucerne",
                        "Winterthur",
                        "St. Gallen",
                        "Zug",
                        "Lugano"
                    }
                },

                {
                    "Germany",
                    new List<string>
                    {
                        "Berlin",
                        "Hamburg",
                        "Munich",
                        "Cologne",
                        "Frankfurt",
                        "Stuttgart",
                        "Dusseldorf",
                        "Leipzig",
                        "Dortmund",
                        "Nuremberg"
                    }
                },

                {
                    "Austria",
                    new List<string>
                    {
                        "Vienna",
                        "Graz",
                        "Linz",
                        "Salzburg",
                        "Innsbruck",
                        "Klagenfurt"
                    }
                },

                {
                    "France",
                    new List<string>
                    {
                        "Paris",
                        "Marseille",
                        "Lyon",
                        "Toulouse",
                        "Nice",
                        "Nantes",
                        "Strasbourg"
                    }
                },

                {
                    "Italy",
                    new List<string>
                    {
                        "Rome",
                        "Milan",
                        "Naples",
                        "Turin",
                        "Florence",
                        "Bologna",
                        "Venice"
                    }
                },

                {
                    "Spain",
                    new List<string>
                    {
                        "Madrid",
                        "Barcelona",
                        "Valencia",
                        "Seville",
                        "Malaga",
                        "Bilbao"
                    }
                },

                {
                    "Netherlands",
                    new List<string>
                    {
                        "Amsterdam",
                        "Rotterdam",
                        "The Hague",
                        "Utrecht",
                        "Eindhoven"
                    }
                },

                {
                    "Belgium",
                    new List<string>
                    {
                        "Brussels",
                        "Antwerp",
                        "Ghent",
                        "Bruges",
                        "Liege"
                    }
                },

                {
                    "United Kingdom",
                    new List<string>
                    {
                        "London",
                        "Manchester",
                        "Birmingham",
                        "Liverpool",
                        "Leeds",
                        "Edinburgh",
                        "Glasgow"
                    }
                }
            };

        public static List<string> GetCountries()
        {
            return CountriesAndCities.Keys
                .OrderBy(country => country)
                .ToList();
        }

        public static List<string> GetCities(string country)
        {
            if (CountriesAndCities.ContainsKey(country))
            {
                return CountriesAndCities[country]
                    .OrderBy(city => city)
                    .ToList();
            }

            return new List<string>();
        }
    }
}
