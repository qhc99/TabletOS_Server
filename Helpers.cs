using System.Runtime.Serialization;

[Serializable]
internal class PayloadData 
{
    private string v1;
    private string v2;


    public PayloadData(string v1, string v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }
}

internal class MyCategoryAlert
{
}

internal class CategoryFiltered
{
}

internal record PostData(DateTime Date, string Name);