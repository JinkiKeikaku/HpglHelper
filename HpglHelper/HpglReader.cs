using System.Text;

namespace HpglHelper
{
    public class HpglReader
    {
        enum State
        {
            None,
            Command,
            Params,
            Label,
        }
        State mState = State.None;
        string mCommand = "";
        List<string> mParams = new();
        HpglPlotter mPlotter = new();
        int mLabelTerminator = 3;
        public List<HpglShape> Shapes=>mPlotter.Shapes;


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        public void Read(TextReader tr, double paperWidth, double paperheight, double millimeterPerUnit)
        {
            mPlotter.InitPlotter(paperWidth, paperheight, millimeterPerUnit);
            var sb = new StringBuilder();
            while (true)
            {
                var c = tr.Read();
                if (c < 0) break;
                if (char.IsWhiteSpace((char)c)) continue;
                switch (mState)
                {
                    case State.None:
                        if (char.IsLetter((char)c))
                        {
                            sb.Clear();
                            sb.Append((char)c);
                            mState = State.Command;
                        }
                        break;
                    case State.Command:
                        if (char.IsLetter((char)c))
                        {
                            sb.Append((char)c);
                            mCommand = sb.ToString();
                            mParams.Clear();
                            sb.Clear();
                            if (mCommand == "LB")
                            {
                                mState = State.Label;
                            }
                            else
                            {
                                mState = State.Params;
                            }
                        }
                        break;
                    case State.Params:
                        if ((char)c == ';')
                        {
                            if (sb.Length > 0) mParams.Add(sb.ToString());
                            sb.Clear();
                            ExecuteCommand();
                            mParams.Clear();
                            mCommand = "";
                            mState = State.None;
                        }
                        else if ((char)c == ',')
                        {
                            mParams.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append((char)c);
                        }
                        break;
                    case State.Label:
                        if (c == (int)mLabelTerminator)
                        {
                            mParams.Add(sb.ToString());
                            sb.Clear();
                            ExecuteCommand();
                            mParams.Clear();
                            mCommand = "";
                            mState = State.None;
                        }
                        else
                        {
                            sb.Append((char)c);
                        }
                        break;
                }
            }

        }
        void ExecuteCommand()
        {
            var cmdActions = new Dictionary<string, Action> {
                { "IN",()=>{ mPlotter.Reset(); } },
                { "DF",()=>{ } },
                { "IP",()=>{ InputP1AndP2(); } },
                { "SC",()=>{ Scale(); } },
                { "SP",()=>{ } },
                { "PU",()=>{ mPlotter.PenUp();   PlotPenMove(); } },
                { "PD",()=>{ mPlotter.PenDown();   PlotPenMove(); } },
                { "PA",()=>{PlotAbsolute(); }},
                { "PR",()=>{PlotRelative(); }},
                { "LT",()=>{ } },
                { "CT",()=>{SetChordToleranceMode(); } },
                { "CI",()=>{PlotCircle(); } },
                { "AA",()=>{PlotArc(false); } },
                { "AR",()=>{PlotArc(true); } },
                { "LB",()=>{Label(); } },
                { "DT",()=>{ DefineTerminator(); } },
                { "SI",()=>{ AbsoluteCharactorSize(); } },
                { "DI",()=>{ AbsoluteDirection(); } },
                { "DR",()=>{ RelativeDirection(); } },
                { "LO",()=>{ LabelOrigin(); } },
                { "CP",()=>{ CharacterMove(); } },
            };

            if (cmdActions.TryGetValue(mCommand, out var action))
            {
                action();
            }
        }
        List<HpglPoint> GetPoints()
        {
            var a = new List<HpglPoint>();
            for (var i = 0; i < mParams.Count - 1; i += 2)
            {
                double.TryParse(mParams[i], out var x);
                double.TryParse(mParams[i + 1], out var y);
                a.Add(new HpglPoint(x, y));
            }
            return a;
        }
        List<double> GetValues()
        {
            var a = new List<double>();
            for (var i = 0; i < mParams.Count; i++)
            {
                double.TryParse(mParams[i], out var x);
                a.Add(x);
            }
            return a;
        }

        void InputP1AndP2()
        {
            var a = GetPoints();
            if (a.Count == 0) mPlotter.SetIP();
            else if (a.Count == 1) mPlotter.SetIP(a[0]);
            else if (a.Count == 2) mPlotter.SetIP(a[0], a[1]);
        }

        void Scale()
        {
            var a = GetValues();
            if (a.Count == 0) mPlotter.SetSC();
            else if(a.Count == 4) mPlotter.SetSC(a[0], a[1], a[2], a[3]);
        }

        void SetChordToleranceMode()
        {
            var a = GetValues();
            mPlotter.ChordToleranceMode = a.Count == 0 ? 0:(int)a[0];
        }

        void PlotPenMove()
        {
            foreach (var p in GetPoints())
            {
                mPlotter.Move(p.X, p.Y);
            }
        }

        void PlotAbsolute()
        {
            foreach (var p in GetPoints())
            {
                mPlotter.MoveAbsolute(p.X, p.Y);
            }
        }
        void PlotRelative()
        {
            foreach (var p in GetPoints())
            {
                mPlotter.MoveRelative(p.X, p.Y);
            }
        }

        void PlotCircle()
        {
            var a = GetValues();
            if (a.Count == 0) return;
            var tolerance = a.Count == 2 ? a[1] : 5;
            mPlotter.Circle(a[0], tolerance);
        }
        void PlotArc(bool isRelative)
        {
            var a = GetValues();
            if (a.Count < 3) return;
            var tolerance = a.Count == 4 ? a[3] : 5;
            mPlotter.Arc(a[0], a[1], a[2], isRelative, tolerance);
        }
        void Label()
        {
            if (mParams.Count == 0) return;
            mPlotter.Label(mParams[0]);
        }
        void DefineTerminator()
        {
            if (mParams.Count == 0 || string.IsNullOrEmpty(mParams[0]))
            {
                mLabelTerminator = 3;
            }
            else
            {
                mLabelTerminator = mParams[0][0];
            }
        }
        void AbsoluteCharactorSize()
        {
            var a = GetValues();
            if (a.Count == 0)
            {
                mPlotter.InitFontSize();
            }
            else
            {
                if (a.Count < 2) return;
                mPlotter.FontWidth = a[0];
                mPlotter.FontHeight = a[1];
            }
        }
        void AbsoluteDirection()
        {
            var a = GetValues();
            if (a.Count == 0)
            {
                mPlotter.TextAngle = 0;
            }
            else
            {
                if (a.Count < 2) return;
                mPlotter.TextAngle = 180 * Math.Atan2(a[1], a[0]) / Math.PI;
            }
        }
        void RelativeDirection()
        {
            var a = GetValues();
            if (a.Count == 0)
            {
                mPlotter.TextAngle = 0;
            }
            else
            {
                if (a.Count < 2) return;

                var dp = mPlotter.P2 - mPlotter.P1;
                mPlotter.TextAngle = 180 * Math.Atan2(a[1] * dp.X, a[0]*dp.Y) / Math.PI;
            }
        }
        void LabelOrigin()
        {
            var a = GetValues();
            if (a.Count == 0)
            {
                mPlotter.LabelOrigin = 1;
            }
            else
            {
                mPlotter.LabelOrigin = (int)a[0];
            }
        }
        void CharacterMove()
        {
            var dx = 0.0;
            var dy = 0.0;
            var a = GetValues();
            if (a.Count == 0)
            {//改行
                dy = -1.0;
            }
            if (a.Count > 0)
            {
                dx = a[0];
            }
            if (a.Count > 1)
            {
                dy = a[1];
            }
            mPlotter.CharacterMove(dx, dy);
        }
    }
}
