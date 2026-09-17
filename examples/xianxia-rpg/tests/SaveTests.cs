using NUnit.Framework;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Persistence;

namespace XianxiaRPG.Tests;

[TestFixture]
public class SaveTests
{
    [Test]
    public void Save_RoundTripsState()
    {
        var data = new SaveData { RegionId = "r1" };
        var c = new CharacterSave { Name = "A", Element = Element.Wind, Hp = 80, MaxHp = 100, Mp = 40, MaxMp = 50, Level = 3, Exp = 50 };
        c.Attr[(int)AttrKey.Attack] = 40;
        c.Attr[(int)AttrKey.Speed] = 60;
        data.Characters.Add(c);
        data.Inventory.Materials.Add(new MaterialSave { Id = "mat_wind", Name = "风精", Stack = 2 });
        data.ClearedSpots.Add(0);
        data.ClearedSpots.Add(1);

        string json = SaveSystem.ToJson(data);
        var back = SaveSystem.FromJson(json);

        Assert.That(back.RegionId, Is.EqualTo("r1"));
        Assert.That(back.Characters[0].Attr[(int)AttrKey.Attack], Is.EqualTo(40));
        Assert.That(back.Characters[0].Attr[(int)AttrKey.Speed], Is.EqualTo(60));
        Assert.That(back.Characters[0].Level, Is.EqualTo(3));
        Assert.That(back.Inventory.Materials[0].Stack, Is.EqualTo(2));
        Assert.That(back.ClearedSpots.Count, Is.EqualTo(2));
    }
}
