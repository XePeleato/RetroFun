﻿using RetroFun.Globals;
using RetroFun.Helpers;
using RetroFun.Subscribers;
using RetroFun.Utils.Furnitures.FloorFurni;
using RetroFun.Utils.Furnitures.WallFurni;
using RetroFun.Utils.Globals;
using Sulakore.Communication;
using Sulakore.Habbo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RetroFun.Handlers
{
    [ToolboxItem(true)]
    [DesignerCategory("UserControl")]
    public partial class FurniHandlerEventPage :  ObservablePage
    {
      
        public FurniHandlerEventPage()
        {
            InitializeComponent();
        }

        private List<HFloorItem> RoomFloorFurni
        {
            get => FloorFurnitures.Furni;
            set
            {
                FloorFurnitures.Furni = value;
                RaiseOnPropertyChanged();
            }
        }

        private List<HWallItem> RoomWallFurni
        {
            get => WallFurnitures.Furni;
            set
            {
                WallFurnitures.Furni = value;
                RaiseOnPropertyChanged();
            }
        }

        public override void In_RoomFloorItems(DataInterceptedEventArgs e)
        {
            RoomFloorFurni = HFloorItem.Parse(e.Packet).ToList(); //All Floor Objects
            e.Continue();
        }


        public override void In_AddFloorItem(DataInterceptedEventArgs e)
        {
            try
            {
                var NewFloorFurnis = new HFloorItem(e.Packet);
                if (!RoomFloorFurni.Contains(NewFloorFurnis))
                {
                    RoomFloorFurni.Add(NewFloorFurnis);
                }
            }
            catch (Exception) { }
        }

        public override void In_RoomWallItems(DataInterceptedEventArgs e)
        {
            if (RecognizeDomain.GetHost(Connection.Host) == RecognizeDomain.bobbaitalia)
            {
                RoomWallFurni = WallFurnitures.BobbaParser(e.Packet);
            }
            else
            {
                RoomWallFurni = HWallItem.Parse(e.Packet).ToList();
            }
        }

        public override void In_AddWallItem(DataInterceptedEventArgs e)
        {
            try
            {
                var NewPlacedWallFurni = new HWallItem(e.Packet);
                if (!RoomWallFurni.Contains(NewPlacedWallFurni))
                {
                    RoomWallFurni.Add(NewPlacedWallFurni);
                }
            }
            catch (Exception) { }
        }


        private void HandleRemovedFurni(HWallItem item)
        {
            if (RoomWallFurni.Contains(item))
            {
                RoomWallFurni.Remove(item);
            }
        }

        private void HandleRemovedFurni(HFloorItem item)
        {
                if (RoomFloorFurni.Contains(item))
                {
                    RoomFloorFurni.Remove(item);
                }
        }


        public override void In_WallItemUpdate(DataInterceptedEventArgs e)
        {
            string furniid = e.Packet.ReadString();
            int typeIdIguess = e.Packet.ReadInteger();
            string newLocation = e.Packet.ReadString();
            if (int.TryParse(furniid, out int furni))
            {
                UpdateFurniMovement(furni, newLocation);
            }
            e.Packet.Position = 0;
            e.Continue();
        }


        public override void Out_RotateMoveItem(DataInterceptedEventArgs e)
        {
            int FurniID = e.Packet.ReadInteger();
            int x = e.Packet.ReadInteger();
            int y = e.Packet.ReadInteger();
            int z = e.Packet.ReadInteger();
            UpdateFurniMovement(FurniID, x, y, z);
            e.Packet.Position = 0;
            e.Continue();

        }

        private void UpdateFurniMovement(HFloorItem furni, int Coord_x, int Coord_y, string Coord_z)
        {
            var roomfurni = RoomFloorFurni.Find(x => x == furni);
            if (roomfurni != null)
            {
                roomfurni.Tile.X = Coord_x;
                roomfurni.Tile.Y = Coord_y;
                if (Double.TryParse(Coord_z, out Double res))
                {
                    roomfurni.Tile.Z = res;
                }
            }       
        }

        private void UpdateFurniMovement(HFloorItem furni, int Coord_x, int Coord_y)
        {
            var roomfurni = RoomFloorFurni.Find(x => x == furni);
            if (roomfurni != null)
            {
                roomfurni.Tile.X = Coord_x;
                roomfurni.Tile.Y = Coord_y;
            }
        }
        private void HandleRemovedFurni(string item)
        {
            if (int.TryParse(item, out int furni))
            {
                var foundfurni = RoomFloorFurni.Find(f => f.Id == furni);
                var wallfurni = RoomWallFurni.Find(f => f.Id == furni);

                if (foundfurni != null)
                {
                    HandleRemovedFurni(foundfurni);
                    return;
                }
                if (wallfurni != null)
                {
                    HandleRemovedFurni(wallfurni);
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void UpdateFurniMovement(int furni, int Coord_x, int Coord_y)
        {
            var roomfurni = RoomFloorFurni.Find(x => x.Id == furni);
            if (roomfurni != null)
            {
                UpdateFurniMovement(roomfurni, Coord_x, Coord_y);
                return;
            }
        }
        private void UpdateFurniMovement(int furni, int Coord_x, int Coord_y, int Coord_z)
        {
            var roomfurni = RoomFloorFurni.Find(x => x.Id == furni);
            if (roomfurni != null)
            {
                UpdateFurniMovement(roomfurni, Coord_x, Coord_y, Coord_z);
                return;
            }
        }

        private void UpdateFurniMovement(HFloorItem furni, int Coord_x, int Coord_y, int Coord_z)
        {
            var roomfurni = RoomFloorFurni.Find(x => x == furni);
            if (roomfurni != null)
            {
                roomfurni.Tile.X = Coord_x;
                roomfurni.Tile.Y = Coord_y;
                roomfurni.Tile.Z = Coord_z;
            }
        }
        private void UpdateFurniMovement(int furni, string wallcoord)
        {
            var roomfurni = RoomWallFurni.Find(x => x.Id == furni);
            if (roomfurni != null)
            {
                UpdateFurniMovement(roomfurni, wallcoord);
                return;
            }
        }

        private void UpdateFurniMovement(HWallItem furni, string wallcoord)
        {
            var roomfurni = RoomWallFurni.Find(x => x == furni);

            if (roomfurni != null)
            {
                roomfurni.Location = wallcoord;
            }
        }

        public override void Out_RequestRoomLoad(DataInterceptedEventArgs e)
        {
            RoomFloorFurni.Clear();
            RoomWallFurni.Clear();

        }


        public override void Out_RequestRoomHeightmap(DataInterceptedEventArgs e)
        {
            RoomFloorFurni.Clear();
            RoomWallFurni.Clear();
        }

        public override void In_RemoveFloorItem(DataInterceptedEventArgs e)
        {
            HandleRemovedFurni(e.Packet.ReadString());
            e.Continue();
        }

        public override void In_RemoveWallItem(DataInterceptedEventArgs e)
        {
            HandleRemovedFurni(e.Packet.ReadString());
            e.Continue();
        }
    }
}