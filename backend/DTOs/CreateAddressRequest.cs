namespace WaiMai.Backend.DTOs
{
    /// <summary>
    /// 创建收货地址请求DTO
    /// 用于接收客户端提交的新增地址信息
    /// </summary>
    public class CreateAddressRequest
    {
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 完整收货地址
        /// </summary>
        public string FullAddress { get; set; } = string.Empty;

        /// <summary>
        /// 是否设置为默认地址
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
