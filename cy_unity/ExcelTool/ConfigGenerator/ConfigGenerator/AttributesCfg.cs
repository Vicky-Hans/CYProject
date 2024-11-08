using Newtonsoft.Json;
using MessagePack;


namespace ConfigGenerator;


[MessagePackObject]
public partial class AttributesCfg
{
    [Key(0)]
    [JsonProperty(PropertyName = "id")]
    public int Id { get; private set; }

    [Key(1)]
    [JsonProperty(PropertyName = "type")]
    public int Type { get; private set; }

    [Key(2)]
    [JsonProperty(PropertyName = "icon")]
    public string Icon { get; private set; }

    [Key(3)]
    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [Key(4)]
    [JsonProperty(PropertyName = "desc")]
    public string Desc { get; private set; }

    [Key(5)]
    [JsonProperty(PropertyName = "show")]
    public int Show { get; private set; }

    [Key(6)]
    [JsonProperty(PropertyName = "showType")]
    public int ShowType { get; private set; }

    [Key(7)]
    [JsonProperty(PropertyName = "unit")]
    public string Unit { get; private set; }

    // public AttributesCfg()
    // {
    // }

    [SerializationConstructor]
    public AttributesCfg(int Id, int Type, string Icon, string Name, string Desc, int Show, int ShowType, string Unit)
    {
        this.Id = Id;
        this.Type = Type;
        this.Icon = Icon;
        this.Name = Name;
        this.Desc = Desc;
        this.Show = Show;
        this.ShowType = ShowType;
        this.Unit = Unit;
    }
}