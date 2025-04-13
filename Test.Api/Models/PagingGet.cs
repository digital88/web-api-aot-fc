using Microsoft.AspNetCore.Mvc;

namespace Test.Api.Models;

public class PagingGet
{
    public const uint MAX_PAGE_SIZE = 10;

    public PagingGet()
    {

    }

    [FromQuery]
    public uint Page { get; set; }

    private uint _pageSize = MAX_PAGE_SIZE;
    [FromQuery]
    public uint PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize = value;
            if (_pageSize > MAX_PAGE_SIZE)
                _pageSize = MAX_PAGE_SIZE;
        }
    }
}

public sealed class PagingGetResponse<TData> : PagingGet
{
    public PagingGetResponse(IEnumerable<TData> data, uint page, uint pageSize)
    {
        Page = page;
        PageSize = pageSize;
        Data = [.. data];
    }

    public IReadOnlyCollection<TData> Data { get; }
}