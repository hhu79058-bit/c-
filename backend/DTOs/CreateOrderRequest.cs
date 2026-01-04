using System.Collections.Generic;

namespace WaiMai.Backend.DTOs
{
    public class CreateOrderRequest
    {
        public int MerchantId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}
