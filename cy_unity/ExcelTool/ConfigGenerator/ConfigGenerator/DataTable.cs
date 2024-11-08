using MessagePack;
using Newtonsoft.Json;

namespace ConfigGenerator;

[MessagePackObject]
public class DataTable<T>
{
    [Key(0)] [JsonProperty(PropertyName = "data")]
    public List<T> Data;
    
    // public DataTable()
    // {
    //         
    // }

    [SerializationConstructor]
    public DataTable(List<T> data)
    {
        this.Data = data;
    }

}