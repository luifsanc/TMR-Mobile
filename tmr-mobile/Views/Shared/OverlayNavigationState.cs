namespace tmr_mobile.Views.Shared;

public static class OverlayNavigationState
{
    private static WeakReference<Page>? _pageToPreserve;
    private static DateTime _preserveUntilUtc;

    public static void PreserveOnNextAppearing(Page page)
    {
        _pageToPreserve = new WeakReference<Page>(page);
        _preserveUntilUtc = DateTime.UtcNow.AddSeconds(3);
    }

    public static bool ConsumePreservation(Page page)
    {
        if (DateTime.UtcNow > _preserveUntilUtc)
        {
            _pageToPreserve = null;
            return false;
        }

        if (_pageToPreserve is null ||
            !_pageToPreserve.TryGetTarget(out var pageToPreserve) ||
            !ReferenceEquals(pageToPreserve, page))
        {
            return false;
        }

        _pageToPreserve = null;
        return true;
    }
}
