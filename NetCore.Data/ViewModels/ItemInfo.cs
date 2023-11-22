namespace NetCore.Data.ViewModels
{
    /// <summary>
    /// 품목정보 클래스
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// 상품번호
        /// </summary>
        public Guid ItemNo { get; set; }

        /// <summary>
        /// 상품명
        /// </summary>
        public required string ItemName { get; set; }
    }
}
