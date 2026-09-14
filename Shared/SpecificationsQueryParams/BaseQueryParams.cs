namespace Shared.SpecificationsQueryParams
{
    public class BaseQueryParams
    {
        private const int MaxPageSize = 100;
        public int PageIndex { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value <= 0 ? 10 : value);
        }

        public string? Search { get; set; }
        public string? Sort { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
