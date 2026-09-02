namespace Vargshala.Web.Common;

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

/// <summary>
/// Reusable 3-state sorting state manager for data tables across all Vargshala pages (Students, Batches, Teachers, Fees, etc.).
/// Cycle: None -> Ascending (▲) -> Descending (▼) -> None (↕)
/// </summary>
public class TableSortState
{
    public string? Column { get; private set; }
    public SortDirection Direction { get; private set; } = SortDirection.None;

    public bool IsSorted => Direction != SortDirection.None && !string.IsNullOrEmpty(Column);

    public void Toggle(string column)
    {
        if (Column == column)
        {
            Direction = Direction switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };

            if (Direction == SortDirection.None)
            {
                Column = null;
            }
        }
        else
        {
            Column = column;
            Direction = SortDirection.Ascending;
        }
    }

    public string GetIndicator(string column)
    {
        if (Column != column || Direction == SortDirection.None)
            return "↕";
        return Direction == SortDirection.Ascending ? "▲" : "▼";
    }

    public string GetIndicatorClass(string column)
    {
        if (Column == column && Direction != SortDirection.None)
            return "text-[#009488] font-bold text-[11px]";
        return "text-slate-300 group-hover:text-slate-400 text-[10px]";
    }

    public void Reset()
    {
        Column = null;
        Direction = SortDirection.None;
    }
}
