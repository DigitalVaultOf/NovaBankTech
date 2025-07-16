namespace Bank.Api.DTOS
{
    public class PagesOfMovimentHistoryDto<T>
    {
        public IEnumerable<T> Pages { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int PageCout => (int)Math.Ceiling((double)TotalCount / (double)PageSize); // O Math.Ceiling é uma função que arredonda os numeros para cima
    }
}
