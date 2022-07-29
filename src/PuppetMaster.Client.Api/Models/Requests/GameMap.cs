using System.ComponentModel;

namespace PuppetMaster.Client.Valorant.Api.Models.Requests
{
    public enum GameMap
    {
        [Description("Ascent")]
        Ascent,

        [Description("Bind")]
        Duality,

        [Description("Split")]
        Bonsai,

        [Description("Haven")]
        Triad,

        [Description("Icebox")]
        Port,

        [Description("Breeze")]
        Foxtrot,

        [Description("Fracture")]
        Canyon,

        [Description("Pearl")]
        Pitt
    }
}
