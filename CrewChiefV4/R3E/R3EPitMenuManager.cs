using CrewChiefV4.RaceRoom.RaceRoomData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.R3E
{    
    // pit menu state:
    /*
    public Int32 Preset;

    // Pit menu actions
    public Int32 Penalty;
    public Int32 Driverchange;
    public Int32 Fuel;
    public Int32 FrontTires;
    public Int32 RearTires;
    public Int32 FrontWing;
    public Int32 RearWing;
    public Int32 Suspension;

    // Pit menu buttons
    public Int32 ButtonTop;
    public Int32 ButtonBottom;
    */

    // mapped directly to game data
    public enum SelectedItem
    {
        Unavailable = -1,

        // Pit menu preset
        Preset = 0,

        // Pit menu actions
        Penalty = 1,
        Driverchange = 2,
        Fuel = 3,
        Fronttires = 4,
        Reartires = 5,
        Frontwing = 6,
        Rearwing = 7,
        Suspension = 8,

        // Pit menu buttons
        ButtonTop = 9,
        ButtonBottom = 10,

        // Pit menu nothing selected
        Max = 11
    }
    
    public class R3EPitMenuManager
    {
        public SelectedItem selectedItem;
        public PitMenuState state;
        public void map(Int32 pitMenuSelection, PitMenuState state)
        {
            this.selectedItem = (SelectedItem) pitMenuSelection;
            this.state = state;
            // print();
        }

        private void print()
        {
            Console.WriteLine("SelectedItem = " + selectedItem +
                " preset = " + state.Preset +
                " penalty = " + state.Penalty +
                " driverchange = " + state.Driverchange +
                " fuel = " + state.Fuel +
                " fronttyres = " + state.FrontTires +
                " reartyres = " + state.RearTires +
                " frontwing = " + state.FrontWing +
                " rearwing = " + state.RearWing +
                " suspension = " + state.Suspension +
                " top = " + state.ButtonTop +
                " bottom = " + state.ButtonBottom);
        }
    }
}
