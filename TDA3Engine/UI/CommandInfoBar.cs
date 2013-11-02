﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TDA3Engine
{
    public class CommandInfoBar
    {
        public Rectangle Rectangle
        {
            get;
            private set;
        }

        public UIBlock PurchaseTower
        {
            get;
            private set;
        }

        public UIBlock SelectedTower
        {
            get;
            private set;
        }

        public UIBlock StatsAndControls
        {
            get;
            private set;
        }

        public UIBlock MoneyAndTowers
        {
            get;
            private set;
        }

        public Session Session
        {
            get;
            private set;
        }

        int padding, waveindex;

        Texture2D background;

        SpriteFont spriteFont;

        Tower clickedTower;

        KeyboardState oldKeyboardState;

        private string respuesta;

        private bool presionado = false;

        public CommandInfoBar(Session s, Rectangle r, GraphicsDevice gd)
        {
            Session = s;
            Session.TowerPurchased += new TDA3Engine.Session.PurhcaseTowerEventHandler(Session_TowerPurchased);
            Session.MoneyIncreased += new EventHandler(Session_MoneyIncreased);
            background = Session.Map.InfoBarBackground;
            Rectangle = r;
            padding = 10;
            waveindex = Session.Map.WaveIndex;

            MoneyAndTowers = new UIBlock(gd, null, s.Map.BorderColor, new Rectangle(r.X, r.Y, r.Width - 5, 50), s);
            PurchaseTower = new UIBlock(gd, s.Map.BorderTexture, s.Map.BorderColor, new Rectangle(r.X, MoneyAndTowers.Dimensions.Bottom + 10, r.Width - 5, 420), s);
            SelectedTower = new UIBlock(gd, s.Map.BorderTexture, s.Map.BorderColor, new Rectangle(r.X, MoneyAndTowers.Dimensions.Bottom + 10, r.Width - 5, 420), s);
            StatsAndControls = new UIBlock(gd, s.Map.BorderTexture, s.Map.BorderColor, new Rectangle(r.X, PurchaseTower.Dimensions.Bottom + 10, r.Width - 5, 200), s);

            s.HealthDecreased += new EventHandler(s_HealthDecreased);
        }

        void Session_MoneyIncreased(object sender, EventArgs e)
        {
            Button bt = SelectedTower.GetButton("BuyTower");
            //Button ut = SelectedTower.GetButton("UpgradeTower");

            if (bt != null)
            {
                if (clickedTower != null && clickedTower.Cost <= Session.ActivePlayer.Money)
                {
                    bt.Texture = Session.Map.SmallNormalButtonTexture;
                    bt.SetColor(Session.Map.ForeColor);

                    if (bt.State == UIButtonState.Inactive)
                    {
                        bt.LeftClickEvent += new EventHandler(buyTower_LeftClick);
                        bt.Activate();
                    }
                }
            }

            /*else if (ut != null)
            {
                if (clickedTower != null && clickedTower.UpgradeCost <= Session.ActivePlayer.Money && clickedTower.Level + 1 < clickedTower.MaxLevel)
                {
                    ut.Texture = Session.Map.SmallNormalButtonTexture;
                    ut.SetColor(Session.Map.ForeColor);

                    if (ut.State == UIButtonState.Inactive)
                    {
                        ut.LeftClickEvent += new EventHandler(upgradeTower_LeftClick);
                        ut.Activate();
                    }
                }
            }*/
        }

        void s_HealthDecreased(object sender, EventArgs e)
        {
            Text t = StatsAndControls.GetText("Lonches");
            t.Value = Session.HealthDisplay;
        }

        void Session_TowerPurchased(object sender, TowerEventArgs ptea)
        {
            ptea.t.LeftClickEvent += new EventHandler(clickableTower_LeftClickEvent);
            Button b = SelectedTower.GetButton("BuyTower");
            if (clickedTower.Cost > Session.ActivePlayer.Money)
            {
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);

                if (b.State == UIButtonState.Active)
                {
                    b.LeftClickEvent -= buyTower_LeftClick;
                    b.Deactivate();
                }
            }
        }

        public void Initialize(SpriteFont sFont)
        {
            spriteFont = sFont;
            InitializeMoneyAndTowers();
            InitializePurchaseTower();
            InitializeStatsAndControls();
        }

        void clickableTower_LeftClickEvent(object sender, EventArgs e)
        {
            Tower t = sender as Tower;
            if (t.IsPlaced && t.PlacedTime > 1)
            {
                clickedTower = t;
                InitializeSelectedTower(t);
            }
        }

        private void InitializeMoneyAndTowers()
        {
            MoneyAndTowers.Add("Dinero", new Text(Session.MoneyDisplay, new Vector2(Rectangle.Left + padding, Rectangle.Top + padding)));
            MoneyAndTowers.Add("Torres", new Text(Session.TowersDisplay, new Vector2(Rectangle.Left + padding, Rectangle.Top + padding + spriteFont.LineSpacing)));
        }

        private void InitializePurchaseTower()
        {
            PurchaseTower.Add("Comprar", new Text("Comprar Torre", new Vector2(PurchaseTower.Dimensions.Left + padding, PurchaseTower.Dimensions.Top + padding)));

            Vector2 pos = new Vector2(PurchaseTower.Dimensions.Left + padding, PurchaseTower.Dimensions.Top + padding + (spriteFont.LineSpacing * 2));
            foreach (Tower t in Session.Map.TowerList)
            {
                Button b = new Button(t.Thumbnail, Vector2.Add(pos, new Vector2(t.Thumbnail.Width / 2.0f, t.Thumbnail.Height / 2.0f)), t);
                b.LeftClickEvent += new EventHandler(selectTower_LeftClick);
                PurchaseTower.Add(t.Name, b);
                pos.X += t.Thumbnail.Width + padding;

                if (pos.X + t.Thumbnail.Width >= PurchaseTower.Dimensions.Right)
                {
                    pos = new Vector2(PurchaseTower.Dimensions.Left + padding, pos.Y + t.Thumbnail.Height + padding);
                }
            }
        }

        void selectTower_LeftClick(object sender, EventArgs e)
        {
            if (clickedTower == null)
            {
                Button b = sender as Button;
                if (b != null)
                {
                    Tower t = b.StoredObject as Tower;
                    if (t != null)
                    {
                        clickedTower = t;
                        InitializeSelectedTower(t);
                    }
                }
            }
        }

        private void InitializeSelectedTower(Tower t)
        {
            SelectedTower.ClearAll();

            Image icon = new Image(clickedTower.Texture, new Vector2(SelectedTower.Dimensions.Left, SelectedTower.Dimensions.Top + padding));
            SelectedTower.Add("TowerIcon", icon);

            //SelectedTower.Add("TowerName", new Text(clickedTower.Name + " " + (clickedTower.Level + 1).ToString(), spriteFont, new Vector2(icon.Rectangle.Right + padding, SelectedTower.Dimensions.Top + padding)));
            //SelectedTower.Add("TowerDescription", new Text(clickedTower.Description, spriteFont, new Vector2(icon.Rectangle.Right + padding, SelectedTower.Dimensions.Top + padding + spriteFont.LineSpacing)));

            Text stats = new Text(clickedTower.CurrentStatistics.ToShortString(), spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, icon.Rectangle.Bottom));
            //SelectedTower.Add("Stats", stats);

            Text specials = new Text(String.Format("Specials: {0}", t.bulletBase.Type == BulletType.Normal ? "None" : t.bulletBase.Type.ToString()),
                spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, stats.Rectangle.Bottom));
            //SelectedTower.Add("Specials", specials);

            Text price = new Text(String.Format("Price: {0}", clickedTower.TotalCost), spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, specials.Rectangle.Bottom));
            //SelectedTower.Add("Price", price);

            if (t.IsPlaced)
            {
                //int pb = AddUpgradeButton(price.Rectangle.Bottom + padding);
                //AddSellButton(pb + padding);
                AddSellButton(padding);
            }
            else
            {
                AddPurchaseButton(price.Rectangle.Bottom + padding);
            }

            string s = t.IsPlaced ? "Anular Seleccion" : "Cancelar";
            Vector2 sdim = spriteFont.MeasureString(s);

            Vector2 cbpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(SelectedTower.Dimensions.Bottom - (Session.Map.SmallNormalButtonTexture.Height / 2.0f) - padding));

            Vector2 ctpos = new Vector2((int)(cbpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                (int)(SelectedTower.Dimensions.Bottom - (Session.Map.SmallNormalButtonTexture.Height + sdim.Y) / 2.0f - padding));

            Button cb = new Button(Session.Map.SmallNormalButtonTexture, cbpos, new Text(s, spriteFont, ctpos), Session.Map.ForeColor, null);
            cb.LeftClickEvent += new EventHandler(cancelButton_LeftClick);
            SelectedTower.Add("Cancel", cb);
        }

        private void AddSellButton(int y)
        {
            Button b = null;
            string st = String.Format("Vender Torre (Recibe {0})", (int)(clickedTower.TotalCost * clickedTower.SellScalar));
            Vector2 stdim = spriteFont.MeasureString(st);
            Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallNormalButtonTexture.Height / 2.0f)));

            Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                (int)(y + (Session.Map.SmallNormalButtonTexture.Height - stdim.Y) / 2.0f));

            b = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text(st, spriteFont, tpos), Session.Map.ForeColor, clickedTower);
            b.LeftClickEvent += new EventHandler(sellTower_LeftClick);
            SelectedTower.Add("SellTower", b);
        }

        /*private int AddUpgradeButton(int y)
        {
            Button b = null;
            if (clickedTower.UpgradeCost <= Session.ActivePlayer.Money && clickedTower.Level + 1 < clickedTower.MaxLevel)
            {
                string bt = String.Format("Upgrade Tower (Costs {0})", clickedTower.UpgradeCost);
                Vector2 btdim = spriteFont.MeasureString(bt);
                Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                    (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallNormalButtonTexture.Height / 2.0f)));

                Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                    (int)(y + (Session.Map.SmallNormalButtonTexture.Height - btdim.Y) / 2.0f));

                b = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text(bt, spriteFont, tpos), Session.Map.ForeColor, clickedTower);
                b.LeftClickEvent += new EventHandler(upgradeTower_LeftClick);
                SelectedTower.Add("UpgradeTower", b);
            }
            else
            {
                string bt = String.Format("Upgrade Tower (Costs {0})", clickedTower.UpgradeCost);
                Vector2 btdim = spriteFont.MeasureString(bt);

                Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallErrorButtonTexture.Width / 2.0f) +
                    (SelectedTower.Dimensions.Width - Session.Map.SmallErrorButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallErrorButtonTexture.Height / 2.0f)));

                Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallErrorButtonTexture.Width / 2.0f + padding),
                    (int)(y + (Session.Map.SmallErrorButtonTexture.Height - btdim.Y) / 2.0f));

                b = new Button(Session.Map.SmallErrorButtonTexture, bpos, new Text(bt, spriteFont, tpos), Session.Map.ErrorColor, clickedTower);
                b.Deactivate();
                SelectedTower.Add("UpgradeTower", b);
            }
            return (int)(b.Position.Y - b.Origin.Y) + b.Texture.Height;
        }*/

        private void AddPurchaseButton(int y)
        {
            if (clickedTower.Cost <= Session.ActivePlayer.Money && clickedTower.Level < clickedTower.MaxLevel)
            {
                string bt = String.Format("Comprar (Cuesta {0})", clickedTower.Cost);
                Vector2 btdim = spriteFont.MeasureString(bt);
                Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                    (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallNormalButtonTexture.Height / 2.0f)));

                Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                    (int)(y + (Session.Map.SmallNormalButtonTexture.Height - btdim.Y) / 2.0f));

                Button b = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text(bt, spriteFont, tpos), Session.Map.ForeColor, clickedTower);
                b.LeftClickEvent += new EventHandler(buyTower_LeftClick);
                SelectedTower.Add("BuyTower", b);
            }
            else
            {
                string bt = String.Format("Comprar (Cuesta {0})", clickedTower.Cost);
                Vector2 btdim = spriteFont.MeasureString(bt);

                Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallErrorButtonTexture.Width / 2.0f) +
                    (SelectedTower.Dimensions.Width - Session.Map.SmallErrorButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallErrorButtonTexture.Height / 2.0f)));

                Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallErrorButtonTexture.Width / 2.0f + padding),
                    (int)(y + (Session.Map.SmallErrorButtonTexture.Height - btdim.Y) / 2.0f));

                Button b = new Button(Session.Map.SmallErrorButtonTexture, bpos, new Text(bt, spriteFont, tpos), Session.Map.ErrorColor, clickedTower);
                b.Deactivate();
                SelectedTower.Add("BuyTower", b);
            }
        }

        void sellTower_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Tower t = b.StoredObject as Tower;
                Session.SellTower(t);
                clickedTower = null;
            }
        }

        /*void upgradeTower_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Tower t = b.StoredObject as Tower;
                Session.UpgradeTower(t);
                b.ButtonText.Value = String.Format("Mejorar (Cuesta {0})", clickedTower.UpgradeCost);
                SelectedTower.GetButton("SellTower").ButtonText.Value = String.Format("Vender (Recibes {0})", (int)(clickedTower.TotalCost * clickedTower.SellScalar));
                SelectedTower.GetText("Stats").Value = clickedTower.CurrentStatistics.ToShortString();
                SelectedTower.GetText("Price").Value = String.Format("Precio: {0}", clickedTower.TotalCost);
                SelectedTower.GetText("TowerName").Value = clickedTower.Name + " " + (clickedTower.Level + 1).ToString();

                if (clickedTower.UpgradeCost > Session.ActivePlayer.Money || clickedTower.Level == clickedTower.MaxLevel)
                {
                    b.Texture = Session.Map.SmallErrorButtonTexture;
                    b.SetColor(Session.Map.ErrorColor);
                    b.LeftClickEvent -= upgradeTower_LeftClick;
                    b.Deactivate();
                }
            }
        }*/

        void buyTower_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Tower t = b.StoredObject as Tower;
                if (t != null)
                {
                    Session.SelectTower(t);
                }
            }
        }

        void cancelButton_LeftClick(object sender, EventArgs e)
        {
            ResetTowerReferences();
            Session.UI.MapRegion.ResetTowerReferences();
        }

        private void InitializeStatsAndControls()
        {
            //Text t;
            int y = StatsAndControls.Dimensions.Top + padding;
            //StatsAndControls.Add("Ola", new Text(String.Format("Ola {0} de {1}", waveindex + 1, Session.Map.WaveList.Count), new Vector2(StatsAndControls.Dimensions.Left + padding, y)));

            Vector2 d = spriteFont.MeasureString(Session.HealthDisplay);
            StatsAndControls.Add("Lonches", new Text(Session.HealthDisplay, new Vector2(StatsAndControls.Dimensions.Right - d.X - padding, y)));

            y += (int)(d.Y + spriteFont.LineSpacing);

            string bt = "Lanzar siguiente ola";
            Vector2 btdim = spriteFont.MeasureString(bt);
            Texture2D tex = Session.Map.State == MapState.WaveDelay ? Session.Map.SmallNormalButtonTexture : Session.Map.SmallErrorButtonTexture;
            Color c = Session.Map.State == MapState.WaveDelay ? Session.Map.ForeColor : Session.Map.ErrorColor;

            Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (tex.Width / 2.0f) +
                (SelectedTower.Dimensions.Width - tex.Width) / 2.0f), (int)(y + (tex.Height / 2.0f)));

            Vector2 tpos = new Vector2((int)(bpos.X - tex.Width / 2.0f + padding),
                (int)(y + (tex.Height - btdim.Y) / 2.0f));

            Button b = new Button(tex, bpos, new Text(bt, spriteFont, tpos), c, clickedTower);
            b.LeftClickEvent += new EventHandler(nextWave_LeftClick);
            StatsAndControls.Add("SiguienteOla", b);


            ////////////////////////////////////// AQUI IRÁ LO DE MULTIPLICAR
            y += tex.Height + padding;


            Random random = new Random();
            int izqui = random.Next(1, 9);
            int dere = random.Next(1, 9);
            tex = Session.Map.LargeNormalButtonTexture;
            int x = (int)(SelectedTower.Dimensions.Left + (tex.Width / 2.0f) + padding);
            c = Session.Map.ForeColor;
            bt = izqui + " X " + dere;
            btdim = spriteFont.MeasureString(bt);
            bpos = new Vector2(x, (int)(y + (tex.Height / 2.0f)));

            tpos = new Vector2((int)(bpos.X - tex.Width / 2.0f + padding),
                (int)(y + (tex.Height - btdim.Y) / 2.0f));

            b = new Button(tex, bpos, new Text(bt, spriteFont, tpos), c, null);
            //b.LeftClickEvent += new EventHandler(pause_LeftClick);
            StatsAndControls.Add("Multiplicar", b);

            
            x += tex.Width;
            respuesta = "";
            bt = respuesta;
            btdim = spriteFont.MeasureString(bt);
            bpos = new Vector2(x, (int)(y + (tex.Height / 2.0f)));

            tpos = new Vector2((int)(bpos.X - tex.Width / 2.0f + padding),
                (int)(y + (tex.Height - btdim.Y) / 2.0f));

            b = new Button(tex, bpos, new Text(bt, spriteFont, tpos), c, null);
            b.LeftClickEvent += new EventHandler(increaseSpeed_LeftClick);
            StatsAndControls.Add("Res", b);

            //x += tex.Width;
            //bt = "Enviar";
            //btdim = spriteFont.MeasureString(bt);
            //bpos = new Vector2(x, (int)(y + (tex.Height / 2.0f)));

            //tpos = new Vector2((int)(bpos.X - tex.Width / 2.0f + padding),
            //    (int)(y + (tex.Height - btdim.Y) / 2.0f));

            //b = new Button(tex, bpos, new Text(bt, spriteFont, tpos), c, null);
            //b.LeftClickEvent += new EventHandler(decreaseSpeed_LeftClick);
            //StatsAndControls.Add("Enviar", b);

            /////////////////////////////////////////////////////////////////////////////////7

        }


        public bool ChecaEnter()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool resultado = (oldKeyboardState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter));
            oldKeyboardState = keyboardState;
            return resultado;
        }

        public bool ChecaTecla(Keys key)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool resultado = false;
            if (oldKeyboardState.IsKeyDown(key) && !presionado)
            {
                resultado = true;
                presionado = true;
            }
            else if (oldKeyboardState.IsKeyUp(key) && presionado)
            {
                presionado = false;
                resultado = false;
            }
            //oldKeyboardState = keyboardState;
            return resultado;
        }

        void pause_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b.ButtonText.Value.Equals("Pause"))
            {
                Session.Pause();
            }
            else
            {
                Session.Resume();
            }
        }

        void increaseSpeed_LeftClick(object sender, EventArgs e)
        {
            if (Session.Speed < Session.MaxSpeed)
            {
                Button b = sender as Button;
                Session.IncreaseSpeed(0.1f);
            }
        }

        void decreaseSpeed_LeftClick(object sender, EventArgs e)
        {
            if (Session.Speed > Session.MinSpeed)
            {
                Button b = sender as Button;
                Session.DecreaseSpeed(0.1f);
            }
        }

        void nextWave_LeftClick(object sender, EventArgs e)
        {
            if (Session.Map.State == MapState.WaveDelay)
            {
                Session.Map.StartNextWaveNow();
            }
        }

        private void ResetTowerReferences()
        {
            if (clickedTower != null) clickedTower = null;
            if (Session.SelectedTower != null) Session.DeselectTower();
        }

        public void Update(GameTime gameTime)
        {
            MoneyAndTowers.GetText("Dinero").Value = Session.MoneyDisplay;
            MoneyAndTowers.GetText("Torres").Value = Session.TowersDisplay;
            Button lnw = StatsAndControls.GetButton("SiguienteOla");
            Texture2D tex = Session.Map.State == MapState.WaveDelay ? Session.Map.SmallNormalButtonTexture : Session.Map.SmallErrorButtonTexture;
            Color c = Session.Map.State == MapState.WaveDelay ? Session.Map.ForeColor : Session.Map.ErrorColor;
            lnw.Texture = tex;
            lnw.SetColor(c);

            if (clickedTower != null)
            {
                foreach (var b in SelectedTower.Buttons)
                {
                    b.Value.Update(gameTime, Session.UI.mouse);
                }
            }
            else
            {
                foreach (var b in PurchaseTower.Buttons)
                {
                    b.Value.Update(gameTime, Session.UI.mouse);
                }
            }

            foreach (var b in StatsAndControls.Buttons)
            {
                b.Value.Update(gameTime, Session.UI.mouse);
            }

            if(ChecaEnter())
                respuesta = "";
            if (ChecaTecla(Keys.D0)) 
                respuesta += "0";
            if (ChecaTecla(Keys.D1)) 
                respuesta += "1";
            if (ChecaTecla(Keys.D2)) 
                respuesta += "2";
            if (ChecaTecla(Keys.D3)) 
                respuesta += "3";
            if (ChecaTecla(Keys.D4)) 
                respuesta += "4";
            if (ChecaTecla(Keys.D5)) 
                respuesta += "5";
            if (ChecaTecla(Keys.D6)) 
                respuesta += "6";
            if (ChecaTecla(Keys.D7)) 
                respuesta += "7";
            if (ChecaTecla(Keys.D8)) 
                respuesta += "8";
            if (ChecaTecla(Keys.D9)) 
                respuesta += "9";

            //Button isb = StatsAndControls.GetButton("IncreaseSpeed");
            //Button dsb = StatsAndControls.GetButton("DecreaseSpeed");
            //if (Session.Speed >= Session.MaxSpeed)
            //{
              //  isb.Texture = Session.Map.LargeErrorButtonTexture;
                //isb.SetColor(Session.Map.ErrorColor);
                //dsb.Texture = Session.Map.LargeNormalButtonTexture;
                //dsb.SetColor(Session.Map.ForeColor);
            //}
            //else if (Session.Speed <= Session.MinSpeed)
            //{
              //  isb.Texture = Session.Map.LargeNormalButtonTexture;
                //isb.SetColor(Session.Map.ForeColor);
                //dsb.Texture = Session.Map.LargeErrorButtonTexture;
                //dsb.SetColor(Session.Map.ErrorColor);
            //}
            //else
            //{
              //  isb.Texture = Session.Map.LargeNormalButtonTexture;
                //isb.SetColor(Session.Map.ForeColor);
                //dsb.Texture = Session.Map.LargeNormalButtonTexture;
                //dsb.SetColor(Session.Map.ForeColor);
            //}

            //if (waveindex != Session.Map.WaveIndex)
            //{
              //  waveindex = Session.Map.WaveIndex;
               // StatsAndControls.GetText("Ola").Value = String.Format("Ola {0} de {1}", waveindex + 1, Session.Map.WaveList.Count);
            //}

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Draw(background, Rectangle, Color.White);
            MoneyAndTowers.Draw(gameTime, spriteBatch, spriteFont);

            if (clickedTower != null)
            {
                SelectedTower.Draw(gameTime, spriteBatch, spriteFont);
            }
            else
            {
                PurchaseTower.Draw(gameTime, spriteBatch, spriteFont);
            }

            StatsAndControls.Draw(gameTime, spriteBatch, spriteFont);
        }
    }
}
