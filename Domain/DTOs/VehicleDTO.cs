namespace MinimalApi.DTOs;
public record VehicleDTO
    {
        public string Brand { get; set; }

        public string Name { get; set; }

        public int Year { get; set; }

        public decimal Price { get; set; }

        public string Color { get; set; }
    }