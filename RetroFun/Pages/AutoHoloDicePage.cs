﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using RetroFun.Controls;
using Sulakore.Communication;
using Sulakore.Modules;
using Sulakore.Components;
using RetroFun.Converter;

namespace RetroFun.Pages
{
    [ToolboxItem(true)]
    [DesignerCategory("UserControl")]
    public partial class AutoHoloDicePage : ObservablePage
    {
        public bool ShouldRollFirst => MatchFirstChk.Checked && DiceHostResult != DiceOneResult;
        public bool ShouldRollSecond => MatchSecondChk.Checked && DiceHostResult != DiceTwoResult;
        public bool ShouldRollThird => MatchThirdChk.Checked && DiceHostResult != DiceThreeResult;
        
        private bool HasHostRolledDice;
        private bool RegistrationCompleted;
        private bool isCoroutineEnabled = true;

        private int _currentDiceTargetIndex = -1;

        private int _diceOneId, _diceTwoId, _diceThreeId, 
            _diceHostId;

        #region Dice Results
        private int _diceOneResult;
        public int DiceOneResult
        {
            get => _diceOneResult;
            set
            {
                _diceOneResult = value;
                RaiseOnPropertyChanged();
            }
        }

        private int _diceTwoResult;
        public int DiceTwoResult
        {
            get => _diceTwoResult;
            set
            {
                _diceTwoResult = value;
                RaiseOnPropertyChanged();
            }
        }

        private int _diceThreeResult;
        public int DiceThreeResult
        {
            get => _diceThreeResult;
            set
            {
                _diceThreeResult = value;
                RaiseOnPropertyChanged();
            }
        }

        private int _hostDiceResult;
        public int DiceHostResult
        {
            get => _hostDiceResult;
            set
            {
                _hostDiceResult = value;
                RaiseOnPropertyChanged();
            }
        }
        #endregion

        private bool _ISHoloDiceCheat;
        public bool ISHolodiceCheat
        {
            get => _ISHoloDiceCheat;
            set
            {
                _ISHoloDiceCheat = value;
                RaiseOnPropertyChanged();
            }
        }

        private bool _IsUserFreezed;
        public bool IsUserFreezed
        {
            get => _IsUserFreezed;
            set
            {
                _IsUserFreezed = value;
                RaiseOnPropertyChanged();
            }
        }

        private List<SKoreButton> _registrationButtons;

        public AutoHoloDicePage()
        {
            InitializeComponent();

            _registrationButtons = new List<SKoreButton> {
                RegisterFirstBtn, RegisterSecondBtn, RegisterThirdBtn, RegisterHostBtn
            };

            RegisterFirstBtn.Click += HandleRegisterClick;
            RegisterSecondBtn.Click += HandleRegisterClick;
            RegisterThirdBtn.Click += HandleRegisterClick;
            RegisterHostBtn.Click += HandleRegisterClick;

            Bind(DiceFirstResTB, "Text", nameof(DiceOneResult), new IntToStringConverter());
            Bind(DiceSecondResTB, "Text", nameof(DiceTwoResult), new IntToStringConverter());
            Bind(DiceThirdResTB, "Text", nameof(DiceThreeResult), new IntToStringConverter());
            Bind(DiceHostResTB, "Text", nameof(DiceHostResult), new IntToStringConverter());

            Bind(AutoHolochbx, "Checked", nameof(ISHolodiceCheat));
            Bind(isUserFreezedCheck, "Checked", nameof(IsUserFreezed));

            if (Program.Master != null)
            {
                Triggers.InAttach(In.ItemExtraData, HandleDiceUpdate);

                Triggers.OutAttach(Out.RoomUserWalk, FreezeUser);
                Triggers.OutAttach(Out.TriggerDice, HandleDiceAction);
                Triggers.OutAttach(Out.CloseDice, HandleDiceAction);
            }
        }

        private void HandleRegisterClick(object sender, EventArgs e)
        {
            var registrationButton = (SKoreButton)sender;
            _currentDiceTargetIndex = _registrationButtons.IndexOf(registrationButton);

            registrationButton.Text = "Waiting for dice..";
            _registrationButtons.ForEach(b => b.Enabled = false);
        }

        private void HandleDiceAction(DataInterceptedEventArgs e)
        {
            if (_currentDiceTargetIndex < 0) return;

            e.IsBlocked = true;
            int id = e.Packet.ReadInteger();

            switch (_currentDiceTargetIndex)
            {
                case 0: _diceOneId = id; break;
                case 1: _diceTwoId = id; break;
                case 2: _diceThreeId = id; break;
                case 3: _diceHostId = id; break;
            }

            Invoke((MethodInvoker)delegate
            {
                _registrationButtons[_currentDiceTargetIndex].Text = id.ToString(); //fuk formatting
                _currentDiceTargetIndex = -1;
                _registrationButtons.ForEach(b => b.Enabled = true);
            });

            Broadcast("Dice registered!");
        }

        private void HandleDiceUpdate(DataInterceptedEventArgs e)
        {
            if (!ISHolodiceCheat) return;

            int id = int.Parse(e.Packet.ReadString());
            e.Packet.ReadInteger();
            string data = e.Packet.ReadString();

            e.Continue();

            if (!int.TryParse(data, out int diceState) || diceState == -1) return;
            
            if (id == _diceHostId)
            {
                DiceHostResult = diceState;
                
                if (diceState == 0)
                {
                    CloseDice(_diceOneId);
                    CloseDice(_diceTwoId);
                    CloseDice(_diceThreeId);
                }
            }
            else if (id == _diceOneId) DiceOneResult = diceState;
            else if (id == _diceTwoId) DiceTwoResult = diceState;
            else if (id == _diceThreeId) DiceThreeResult = diceState;
            else return;

            if (diceState < 1) return;

            //These are usually very confusing if you were to read this code like after a month, or someone who has no idea what this is supposed to do were to read this. ik
            if ((!ShouldRollFirst && !MatchSecondChk.Checked && !MatchThirdChk.Checked) ||
                (!ShouldRollFirst && !ShouldRollSecond && !MatchThirdChk.Checked) ||
                (!ShouldRollFirst && !ShouldRollSecond && !ShouldRollThird))
            {
                //WON! Do the victory procedure here.


                Connection.SendToServerAsync(Out.RoomUserShout, holoDiceShoutPhrase.Text, 0);
            }
            else
            {
                if (ShouldRollFirst)
                    RollDice(_diceOneId);

                if (ShouldRollSecond)
                    RollDice(_diceTwoId);

                if (ShouldRollThird)
                    RollDice(_diceThreeId);
            }
        }

        private void FreezeUser(DataInterceptedEventArgs e)
        {
            e.IsBlocked = IsUserFreezed;
        }
  
        private void ClearButton_Click(object sender, EventArgs e)
        {
            _diceOneId = _diceTwoId = _diceThreeId = _diceHostId = -1;
        }

        private void RollDice(int diceID)
        {
            Connection.SendToServerAsync(Out.TriggerDice, diceID);
        }
        private void CloseDice(int diceID)
        {
            Connection.SendToServerAsync(Out.CloseDice, diceID);
        }

        private void Broadcast(string text)
        {
            Connection.SendToClientAsync(In.RoomUserWhisper, 0, "[HoloCheat]: " + text, 0, 34, 0, -1);
        }
    }
}