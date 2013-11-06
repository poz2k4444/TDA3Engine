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

        public UIBlock TablasMultiplicar
        {
            get;
            private set;
        }

        public UIBlock SelectTablas
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
        int ElapsedTime = 15;
        int PreguntaTime = 600;
        int PoderesTotales = 0;
        int RespCorrecta = 0;
        Random random = new Random();
        int dere = 0;
        int izqui = 0;
        List<int> tablasSeleccionadas = new List<int>();
        private bool selectTablas = false;

        KeyboardState oldKeyboardState = Keyboard.GetState();

        private string respuesta = "";

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
            TablasMultiplicar = new UIBlock(gd, null, s.Map.BorderColor, new Rectangle(r.X, r.Y, r.Width - 5, 50), s);
            SelectTablas = new UIBlock(gd, null, s.Map.BorderColor, new Rectangle(r.X, r.Y, r.Width - 5, 300), s);

            s.HealthDecreased += new EventHandler(s_HealthDecreased);
        }

        void Session_MoneyIncreased(object sender, EventArgs e)
        {
            //Button bt = SelectedTower.GetButton("BuyTower");
            Button ut = SelectedTower.GetButton("UpgradeTower");

            /*if (bt != null)
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

            else */if (ut != null)
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
            }
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
            InitializeTablas();
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

            SelectedTower.Add("TowerName", new Text(clickedTower.Name + " " + (clickedTower.Level + 1).ToString(), spriteFont, new Vector2(icon.Rectangle.Right + padding, SelectedTower.Dimensions.Top + padding)));
            SelectedTower.Add("TowerDescription", new Text(clickedTower.Description, spriteFont, new Vector2(icon.Rectangle.Right + padding, SelectedTower.Dimensions.Top + padding + spriteFont.LineSpacing)));

            Text stats = new Text(clickedTower.CurrentStatistics.ToShortString(), spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, icon.Rectangle.Bottom));
            SelectedTower.Add("Stats", stats);

            Text specials = new Text(String.Format("Specials: {0}", t.bulletBase.Type == BulletType.Normal ? "None" : t.bulletBase.Type.ToString()),
                spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, stats.Rectangle.Bottom));
            SelectedTower.Add("Specials", specials);

            Text price = new Text(String.Format("Price: {0}", clickedTower.TotalCost), spriteFont, new Vector2(SelectedTower.Dimensions.Left + padding, specials.Rectangle.Bottom));
            SelectedTower.Add("Price", price);

            if (t.IsPlaced)
            {
                int pb = AddUpgradeButton(price.Rectangle.Bottom + padding);
                //AddSellButton(pb + padding);
                //AddSellButton(padding);
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
            Button c = null;
            string st = String.Format("Vender Torre (Recibe {0})", (int)(clickedTower.TotalCost * clickedTower.SellScalar));
            Vector2 stdim = spriteFont.MeasureString(st);
            Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallNormalButtonTexture.Height / 2.0f)));

            Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                (int)(y + (Session.Map.SmallNormalButtonTexture.Height - stdim.Y) / 2.0f));
            tpos.Y = 425;
            bpos.Y = tpos.Y-2;
            tpos.Y = 410;
            b = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text(st, spriteFont, tpos), Session.Map.ForeColor, clickedTower);
            tpos.Y = 390;
            bpos.Y = tpos.Y - 2;
            tpos.Y = 375;
            c = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text("Poder", spriteFont, tpos), Session.Map.ForeColor, clickedTower);
            b.LeftClickEvent += new EventHandler(sellTower_LeftClick);
            c.LeftClickEvent += new EventHandler(powerTower_LeftClick);
            SelectedTower.Add("SellTower", b);
            SelectedTower.Add("PowerTower", c);
        }
        
        private int AddUpgradeButton(int y)
        {
            Button b = null;
            if (clickedTower.UpgradeCost <= PoderesTotales)
            {
                string bt = String.Format("Poderes: {0}", clickedTower.UpgradeCost);
                Vector2 btdim = spriteFont.MeasureString(bt);
                Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (Session.Map.SmallNormalButtonTexture.Width / 2.0f) +
                    (SelectedTower.Dimensions.Width - Session.Map.SmallNormalButtonTexture.Width) / 2.0f), (int)(y + (Session.Map.SmallNormalButtonTexture.Height / 2.0f)));

                Vector2 tpos = new Vector2((int)(bpos.X - Session.Map.SmallNormalButtonTexture.Width / 2.0f + padding),
                    (int)(y + (Session.Map.SmallNormalButtonTexture.Height - btdim.Y) / 2.0f));

                b = new Button(Session.Map.SmallNormalButtonTexture, bpos, new Text(bt, spriteFont, tpos), Session.Map.ForeColor, clickedTower);
                b.LeftClickEvent += new EventHandler(powerTower_LeftClick);
                SelectedTower.Add("UpgradeTower", b);
            }
            else
            {
                string bt = String.Format("Poderes: {0}", clickedTower.UpgradeCost);
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
        }

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
        void powerTower_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                
                Tower t = b.StoredObject as Tower;
                PoderesTotales = PoderesTotales - clickedTower.UpgradeCost;
                Session.UpgradeTower(t);
                t.Level = 2;
                b.ButtonText.Value = String.Format("Poderes: {0}", clickedTower.UpgradeCost);
                //SelectedTower.GetButton("SellTower").ButtonText.Value = String.Format("Vender (Recibes {0})", (int)(clickedTower.TotalCost * clickedTower.SellScalar));
                //SelectedTower.GetText("Stats").Value = clickedTower.CurrentStatistics.ToShortString();
                //SelectedTower.GetText("Price").Value = String.Format("Precio: {0}", clickedTower.TotalCost);
                //SelectedTower.GetText("TowerName").Value = clickedTower.Name + " " + (clickedTower.Level + 1).ToString();

                if (clickedTower.UpgradeCost > PoderesTotales && t.Level == 2)
                {
                    b.Texture = Session.Map.SmallErrorButtonTexture;
                    b.SetColor(Session.Map.ErrorColor);
                    b.LeftClickEvent -= powerTower_LeftClick;
                    t.Level = 1;
                    b.Deactivate();
                }
            }
            //if (PoderesTotales > 0)
            //{
            //    PoderesTotales--;
            //}
        }

        void upgradeTower_LeftClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Tower t = b.StoredObject as Tower;
                Session.UpgradeTower(t);
                b.ButtonText.Value = String.Format("Mejorar (Cuesta {0})", clickedTower.UpgradeCost);
                //SelectedTower.GetButton("SellTower").ButtonText.Value = String.Format("Vender (Recibes {0})", (int)(clickedTower.TotalCost * clickedTower.SellScalar));
                //SelectedTower.GetText("Stats").Value = clickedTower.CurrentStatistics.ToShortString();
                //SelectedTower.GetText("Price").Value = String.Format("Precio: {0}", clickedTower.TotalCost);
                //SelectedTower.GetText("TowerName").Value = clickedTower.Name + " " + (clickedTower.Level + 1).ToString();

                if (clickedTower.UpgradeCost > PoderesTotales && t.Level >= 2)
                {
                    b.Texture = Session.Map.SmallErrorButtonTexture;
                    b.SetColor(Session.Map.ErrorColor);
                    b.LeftClickEvent -= upgradeTower_LeftClick;
                    b.Deactivate();
                }
            }
        }

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

            TablasMultiplicar.Add("Pregunta", new Text("", new Vector2(1017,607)));
            TablasMultiplicar.Add("Respuesta", new Text("", new Vector2(1117, 607)));
            TablasMultiplicar.Add("Poder", new Text(""+PoderesTotales, new Vector2(1000, 501)));
            CambiarTabla();

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

        private void InitializeTablas()
        {
            int y = StatsAndControls.Dimensions.Top + padding;
            Vector2 d = spriteFont.MeasureString(Session.HealthDisplay);
            y += (int)(d.Y + spriteFont.LineSpacing);
            string bt = "Lanzar siguiente ola";
            Vector2 btdim = spriteFont.MeasureString(bt);
            Texture2D tex = Session.Map.State == MapState.WaveDelay ? Session.Map.SmallNormalButtonTexture : Session.Map.SmallErrorButtonTexture;
            Color c = Session.Map.State == MapState.WaveDelay ? Session.Map.ForeColor : Session.Map.ErrorColor;

            Vector2 bpos = new Vector2((int)(SelectedTower.Dimensions.Left + (tex.Width / 2.0f) +
                (SelectedTower.Dimensions.Width - tex.Width) / 2.0f), (int)(y + (tex.Height / 2.0f)));

            Vector2 tpos = new Vector2((int)(bpos.X - tex.Width / 2.0f + padding),
                (int)(y + (tex.Height - btdim.Y) / 2.0f));

            Button btnT1 = new Button(tex, new Vector2(bpos.X, 150), new Text("Tabla del 1", spriteFont, new Vector2(bpos.X - 62, 135)), c, clickedTower);
            Button btnT2 = new Button(tex, new Vector2(bpos.X, 200), new Text("Tabla del 2", spriteFont, new Vector2(bpos.X - 62, 185)), c, clickedTower);
            Button btnT3 = new Button(tex, new Vector2(bpos.X, 250), new Text("Tabla del 3", spriteFont, new Vector2(bpos.X - 62, 235)), c, clickedTower);
            Button btnT4 = new Button(tex, new Vector2(bpos.X, 300), new Text("Tabla del 4", spriteFont, new Vector2(bpos.X - 62, 285)), c, clickedTower);
            Button btnT5 = new Button(tex, new Vector2(bpos.X, 350), new Text("Tabla del 5", spriteFont, new Vector2(bpos.X - 62, 335)), c, clickedTower);
            Button btnT6 = new Button(tex, new Vector2(bpos.X, 400), new Text("Tabla del 6", spriteFont, new Vector2(bpos.X - 62, 385)), c, clickedTower);
            Button btnT7 = new Button(tex, new Vector2(bpos.X, 450), new Text("Tabla del 7", spriteFont, new Vector2(bpos.X - 62, 435)), c, clickedTower);
            Button btnT8 = new Button(tex, new Vector2(bpos.X, 500), new Text("Tabla del 8", spriteFont, new Vector2(bpos.X - 62, 485)), c, clickedTower);
            Button btnT9 = new Button(tex, new Vector2(bpos.X, 550), new Text("Tabla del 9", spriteFont, new Vector2(bpos.X - 62, 535)), c, clickedTower);
            Button btnT10 = new Button(tex, new Vector2(bpos.X, 650), new Text("iniciar", spriteFont, new Vector2(bpos.X - 62, 635)), c, clickedTower);
            btnT1.LeftClickEvent += new EventHandler(tablasSeleccionda1_LeftClick);
            btnT2.LeftClickEvent += new EventHandler(tablasSeleccionda2_LeftClick);
            btnT3.LeftClickEvent += new EventHandler(tablasSeleccionda3_LeftClick);
            btnT4.LeftClickEvent += new EventHandler(tablasSeleccionda4_LeftClick);
            btnT5.LeftClickEvent += new EventHandler(tablasSeleccionda5_LeftClick);
            btnT6.LeftClickEvent += new EventHandler(tablasSeleccionda6_LeftClick);
            btnT7.LeftClickEvent += new EventHandler(tablasSeleccionda7_LeftClick);
            btnT8.LeftClickEvent += new EventHandler(tablasSeleccionda8_LeftClick);
            btnT9.LeftClickEvent += new EventHandler(tablasSeleccionda9_LeftClick);
            btnT10.LeftClickEvent += new EventHandler(iniciarJuego_LeftClick);

            SelectTablas.Add("Tabla1", btnT1);
            SelectTablas.Add("Tabla2", btnT2);
            SelectTablas.Add("Tabla3", btnT3);
            SelectTablas.Add("Tabla4", btnT4);
            SelectTablas.Add("Tabla5", btnT5);
            SelectTablas.Add("Tabla6", btnT6);
            SelectTablas.Add("Tabla7", btnT7);
            SelectTablas.Add("Tabla8", btnT8);
            SelectTablas.Add("Tabla9", btnT9);
            SelectTablas.Add("Iniciar", btnT10);
        }

        public void CambiarTabla()
        {
            if (is_wave_active())
            {
                do
                {
                    izqui = random.Next(1, 10);
                } while (!tablasSeleccionadas.Contains(izqui));

                /*if( Session.Map.Difficulty == 1)
                    izqui = random.Next(1, 3);
                else if (Session.Map.Difficulty == 2)
                    izqui = random.Next(1, 6);
                else if( Session.Map.Difficulty == 3)
                    izqui = random.Next(1, 9);*/
                dere = random.Next(1, 10);
                RespCorrecta = izqui * dere;
                string valorA = izqui + " X " + dere + " = ";
                TablasMultiplicar.GetText("Pregunta").Value = valorA;
            }
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

        bool is_wave_active()
        {
            if (Session.Map.State == MapState.Active)
            {
                return true;
            }
            return false;
        }

        void nextWave_LeftClick(object sender, EventArgs e)
        {
            if (Session.Map.State == MapState.WaveDelay)
            {
                Session.Map.StartNextWaveNow();
            }
        }

        void tablasSeleccionda1_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(1))
            {
                tablasSeleccionadas.Remove(1);
                Button b = SelectTablas.GetButton("Tabla1");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else {
                tablasSeleccionadas.Add(1);
                Button b = SelectTablas.GetButton("Tabla1");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda2_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(2))
            {
                tablasSeleccionadas.Remove(2);
                Button b = SelectTablas.GetButton("Tabla2");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(2);
                Button b = SelectTablas.GetButton("Tabla2");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda3_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(3))
            {
                tablasSeleccionadas.Remove(3);
                Button b = SelectTablas.GetButton("Tabla3");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(3);
                Button b = SelectTablas.GetButton("Tabla3");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda4_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(4))
            {
                tablasSeleccionadas.Remove(4);
                Button b = SelectTablas.GetButton("Tabla4");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(4);
                Button b = SelectTablas.GetButton("Tabla4");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda5_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(5))
            {
                tablasSeleccionadas.Remove(5);
                Button b = SelectTablas.GetButton("Tabla5");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(5);
                Button b = SelectTablas.GetButton("Tabla5");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda6_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(6))
            {
                tablasSeleccionadas.Remove(6);
                Button b = SelectTablas.GetButton("Tabla6");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(6);
                Button b = SelectTablas.GetButton("Tabla6");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda7_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(7))
            {
                tablasSeleccionadas.Remove(7);
                Button b = SelectTablas.GetButton("Tabla7");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(7);
                Button b = SelectTablas.GetButton("Tabla7");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda8_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(8))
            {
                tablasSeleccionadas.Remove(8);
                Button b = SelectTablas.GetButton("Tabla8");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(8);
                Button b = SelectTablas.GetButton("Tabla8");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void tablasSeleccionda9_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Contains(9))
            {
                tablasSeleccionadas.Remove(9);
                Button b = SelectTablas.GetButton("Tabla9");
                b.Texture = Session.Map.SmallNormalButtonTexture;
                b.SetColor(Session.Map.ForeColor);
            }
            else
            {
                tablasSeleccionadas.Add(9);
                Button b = SelectTablas.GetButton("Tabla9");
                b.Texture = Session.Map.SmallErrorButtonTexture;
                b.SetColor(Session.Map.ErrorColor);
                b.Deactivate();
            }
        }

        void iniciarJuego_LeftClick(object sender, EventArgs e)
        {
            if (tablasSeleccionadas.Count() != 0)
            {
                InitializeMoneyAndTowers();
                InitializePurchaseTower();
                InitializeStatsAndControls();
                selectTablas = true;

                Button b1 = SelectTablas.GetButton("Tabla1");
                b1.LeftClickEvent -= tablasSeleccionda1_LeftClick;
                Button b2 = SelectTablas.GetButton("Tabla2");
                b2.LeftClickEvent -= tablasSeleccionda2_LeftClick;
                Button b3 = SelectTablas.GetButton("Tabla3");
                b3.LeftClickEvent -= tablasSeleccionda3_LeftClick;
                Button b4 = SelectTablas.GetButton("Tabla4");
                b4.LeftClickEvent -= tablasSeleccionda4_LeftClick;
                Button b5 = SelectTablas.GetButton("Tabla5");
                b5.LeftClickEvent -= tablasSeleccionda5_LeftClick;
                Button b6 = SelectTablas.GetButton("Tabla6");
                b6.LeftClickEvent -= tablasSeleccionda6_LeftClick;
                Button b7 = SelectTablas.GetButton("Tabla7");
                b7.LeftClickEvent -= tablasSeleccionda7_LeftClick;
                Button b8 = SelectTablas.GetButton("Tabla8");
                b8.LeftClickEvent -= tablasSeleccionda8_LeftClick;
                Button b9 = SelectTablas.GetButton("Tabla9");
                b9.LeftClickEvent -= tablasSeleccionda9_LeftClick;


            }
        }

        private void ResetTowerReferences()
        {
            if (clickedTower != null) clickedTower = null;
            if (Session.SelectedTower != null) Session.DeselectTower();
        }

        public void Update(GameTime gameTime)
        {
            foreach (var b in SelectTablas.Buttons)
            {
                b.Value.Update(gameTime, Session.UI.mouse);
            }
            if (selectTablas)
            {
                MoneyAndTowers.GetText("Dinero").Value = Session.MoneyDisplay;
                MoneyAndTowers.GetText("Torres").Value = Session.TowersDisplay;
                Button lnw = StatsAndControls.GetButton("SiguienteOla");
                Texture2D tex = Session.Map.State == MapState.WaveDelay ? Session.Map.SmallNormalButtonTexture : Session.Map.SmallErrorButtonTexture;
                Color c = Session.Map.State == MapState.WaveDelay ? Session.Map.ForeColor : Session.Map.ErrorColor;
                lnw.Texture = tex;
                lnw.SetColor(c);

                if ((object)clickedTower != null && clickedTower.IsPlaced)
                {
                    if (PoderesTotales >= clickedTower.UpgradeCost && clickedTower.Level <= 1 && !clickedTower.poder)
                    {
                        clickedTower.Level = 2;
                        Button power = SelectedTower.GetButton("UpgradeTower");
                        power.Texture = Session.Map.SmallNormalButtonTexture;
                        power.SetColor(Session.Map.ForeColor);
                        power.LeftClickEvent += powerTower_LeftClick;

                    }
                }

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

                if (is_wave_active())
                {
                    TablasMultiplicar.GetText("Respuesta").Value = respuesta;
                    TablasMultiplicar.GetText("Poder").Value = "Poderes: " + PoderesTotales;
                    if (oldKeyboardState.IsKeyDown(Keys.Back) && respuesta.Length > 0 && ElapsedTime <= 0)
                    {
                        respuesta = respuesta.Remove(respuesta.Length - 1);
                        ElapsedTime = 15;
                    }
                    if (respuesta.Length <= 2)
                    {
                        if (oldKeyboardState.IsKeyDown(Keys.D0) && ElapsedTime <= 0)
                        {
                            respuesta += "0";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D1) && ElapsedTime <= 0)
                        {
                            respuesta += "1";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D2) && ElapsedTime <= 0)
                        {
                            respuesta += "2";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D3) && ElapsedTime <= 0)
                        {
                            respuesta += "3";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D4) && ElapsedTime <= 0)
                        {
                            respuesta += "4";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D5) && ElapsedTime <= 0)
                        {
                            respuesta += "5";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D6) && ElapsedTime <= 0)
                        {
                            respuesta += "6";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D7) && ElapsedTime <= 0)
                        {
                            respuesta += "7";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D8) && ElapsedTime <= 0)
                        {
                            respuesta += "8";
                            ElapsedTime = 15;
                        }
                        else if (oldKeyboardState.IsKeyDown(Keys.D9) && ElapsedTime <= 0)
                        {
                            respuesta += "9";
                            ElapsedTime = 15;
                        }

                    }
                    if (oldKeyboardState.IsKeyDown(Keys.Enter) && ElapsedTime <= 0)
                    {
                        //Validar respuesta
                        if (respuesta != "correcto" && respuesta != "falso" && respuesta != "" && RespCorrecta == int.Parse(respuesta))
                        {
                            if (PoderesTotales < 5)
                                PoderesTotales++;
                            respuesta = "correcto";
                        }
                        else
                        {
                            respuesta = "falso";
                        }
                        ElapsedTime = 15;
                    }
                    if (PreguntaTime == 0)
                    {
                        PreguntaTime = 600;
                        respuesta = "";
                        CambiarTabla();
                    }
                    ElapsedTime--;
                    PreguntaTime--;

                    oldKeyboardState = Keyboard.GetState();
                }
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
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.Draw(background, Rectangle, Color.White);
            if (selectTablas)
            {
                MoneyAndTowers.Draw(gameTime, spriteBatch, spriteFont);
                TablasMultiplicar.Draw(gameTime, spriteBatch, spriteFont);

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
            else
            {
                SelectTablas.Draw(gameTime, spriteBatch, spriteFont);
            }
        }
    }
}
