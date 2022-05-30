using UnityEngine;
using Utility;

namespace Control
{
    #region Solvers
    /// <summary>
    /// Solver algorithm for solving time invariant differential equations.
    /// </summary>
    /// <param name="x">Current state.</param>
    /// <param name="dx">Derivative of current state.</param>
    /// <param name="h">Timestep.</param>
    /// <returns></returns>
    public delegate Matrix Solver(Matrix x, Matrix u, StateFunction f, float h);
    public static class Solvers
    {
        /// <summary>
        /// Use this solver if your system is discretized.
        /// </summary>
        public static Matrix Discrete(Matrix x, Matrix u, StateFunction f, float h)
        {
            return f(x,u);
        }

        /// <summary>
        /// Fast, inaccurate solver for continuous systems.
        /// </summary>
        public static Matrix ExplicitEuler(Matrix x, Matrix u, StateFunction f, float h)
        {
            return x + h * f(x,u);
        }

        #region RK4 ButcherScheme
        static float[,] Afloats = {
                { 0.5f, 0f, 0f },
                { 0f, 0.5f, 0f },
                { 0f, 0f, 1.0f }
            };

        static float[,] bfloats = {
                { 1f/6f, 1f/3f, 1f/3f, 1f/6f }
            };

        static float[,] cfloats = {
                { 0f },
                { 0.5f },
                { 0.5f },
                { 1f }
            };

        static Matrix A = new Matrix(Afloats);
        static Matrix b = new Matrix(bfloats);
        static Matrix c = new Matrix(cfloats);
        #endregion
        /// <summary>
        /// Runge-Kutta method of order 4. Slower, accurate solver for continuous systems. MATLAB standard.
        /// </summary>
        public static Solver RK4 = CreateFromButcher(new Butcher(A, b, c));

        /// <summary>
        /// Create a solver from its butcher scheme.
        /// </summary>
        public static Solver CreateFromButcher(Butcher butcher)
        {
            int s = b.cols;

            return delegate (Matrix x, Matrix u, StateFunction f, float h) {

                Matrix[] K = new Matrix[s];
                Matrix sum;

                for (int j = 0; j < s; j++) {
                    sum = Matrix.Zero(x.rows, 1);
                    for (int l = 0; l < j; l++) {
                        sum += A.vals[j-1, l] * K[l];
                    }
                    K[j] = f(x + h * sum, u);
                }


                Matrix res = x;
                for (int i = 0; i < s; i++) {
                    res += butcher.b.vals[0, i] * K[i];
                }

                return res;
            };
        }
    }

    public class Butcher
    {
        public Matrix A;
        public Matrix b;
        public Matrix c;

        /// <summary>
        /// Create a Butcher scheme so you can use it to generate a solver from it.
        /// A must be a quadratic lower triangle matrix with one less row and col as b has cols. Matrix c must have as many rows as b has cols.
        /// </summary>
        public Butcher(Matrix A, Matrix b, Matrix c)
        {
            if (!A.IsLowerTriangle()) {
                Debug.LogError("Matrix A must be a quadratic lower triangle matrix for the butcher scheme!");
                throw new System.Exception();
            }

            if (b.rows != 1) {
                Debug.LogError("Matrix b must have exactly one row!");
                throw new System.Exception();
            }

            if (c.cols != 1) {
                Debug.LogError("Matrix c must have exactly one col!");
                throw new System.Exception();
            }

            if (b.cols != c.rows) {
                Debug.LogError("Matrix c must have exactly as many rows as b has cols!");
                throw new System.Exception();
            }

            if (b.cols-1 != A.cols) {
                Debug.LogError("Matrix A must have exactly one less row and col as b has cols!");
                throw new System.Exception();
            }

            this.A = A;
            this.b = b;
            this.c = c;
        }
    }
    #endregion

    #region Systems
    /// <summary>
    /// <para>Is used to define both the state function f(x,u) and the output function g(x,u) of the system.</para>
    /// <para>dx/dt = f(x,u)</para>
    /// <para>y = g(x,u)</para>
    /// </summary>
    /// <param name="x">States</param>
    /// <param name="u">Inputs</param>
    public delegate Matrix StateFunction(Matrix x, Matrix u);

    public class Integrator
    {
        /// <summary>
        /// Get or set the current internal state.
        /// </summary>
        public float state;

        /// <summary>
        /// Integrates its inputs.
        /// </summary>
        /// <param name="state">Optional initial state of the integrator.</param>
        public Integrator(float state = 0f)
        {
            this.state = state;
        }

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        /// <param name="deltaTime">Time since last evaluation. Should be Time.fixedDeltaTime, if you evaluate every FixedUpdate.</param>
        public float Eval(float input)
        {
            state += input * Time.fixedDeltaTime;
            return state;
        }
    }

    public class Differentiator
    {
        float prevInput;
        bool init; // Prevents spike from input signal jump at the beginning

        /// <summary>
        /// Differentiates its inputs. Noisy inputs result in even noisier outputs.
        /// </summary>
        public Differentiator()
        {
            init = false;
        }

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        public float Eval(float input)
        {
            if (!init) {
                init = true;
                prevInput = input;
                return 0.0f;
            } else {
                float res = (input - prevInput) / Time.fixedDeltaTime;
                prevInput = input;
                return res;
            }
        }
    }

    public class Lowpass
    {
		float a;
		public float state;

        /// <summary>
        /// Discrete lowpass filter.
        /// </summary>
        /// <param name="T">Time constant. Time, after which the output reaches 63.2% of a step input.</param>
        public Lowpass(float T)
        {
			if (T < Time.fixedDeltaTime) {
				Debug.LogError("Time constant is smaller than Time.fixedDeltaTime. This will lead to instability.");
			}

			a = Mathf.Exp(-Time.fixedDeltaTime / T);

		}

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        public float Eval(float input)
        {
			float y = state;
			state = a * state + (1 - a) * input;
			return y;
        }
    }

    public class HigherOrderLowpass
    {
        Lowpass[] lows;

        /// <summary>
        /// Applies a lowpass filter of a higher order to the input. It does this by chaining multiple lowpass filters.
        /// </summary>
        /// <param name="T">Time constant for all of the lowpass filters. Time, after which the output reaches 63.2% of a step input, when using a single lowpass filter.</param>
        /// <param name="order">Order of the filter or number of lowpass filters chained together. Has to be a positive integer.</param>
        public HigherOrderLowpass(float T, int order)
        {
            if (order < 1) {
                Debug.LogError("Order has to a positive integer.");
                throw new System.Exception();
            }

            lows = new Lowpass[order];
            for (int i = 0; i < order; i++) {
                lows[i] = new Lowpass(T);
            }
        }

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        public float Eval(float input)
        {
            float val = lows[0].Eval(input);
            for (int i = 1; i < lows.Length; i++) {
                val = lows[i].Eval(val);
            }

            return val;
        }
    }

    public class LTISystem
    {
        public enum CanonicalForm { ControllableCanonicalForm, ObservableCanonicalForm };

        public Matrix A { get; private set; }
        public Matrix B { get; private set; }
        public Matrix C { get; private set; }
        public Matrix D { get; private set; }

        public int n
        {
            get {
                return A.cols;
            }
        }
        public int m
        {
            get {
                return B.cols;
            }
        }
        public int p
        {
            get {
                return C.rows;
            }
        }

        public Matrix x { get; private set; }

        /// <summary>
        /// <para>Define a LTI system with no feedthrough matrix D.</para>
        /// <para>dx/dt = Ax + Bu</para>
        /// <para>y = Cx</para>
        /// </summary>
        /// <param name="A">State matrix.</param>
        /// <param name="B">Input matrix.</param>
        /// <param name="C">Output matrix.</param>
        public LTISystem(Matrix A, Matrix B, Matrix C)
        {
            int m = B.cols; // Inputs
            int p = C.rows; // Outputs

            Matrix D = Matrix.Zero(p, m);
            Init(A, B, C, D);
        }

        /// <summary>
        /// <para>Define a LTI system with feedthrough matrix D.</para>
        /// <para>dx/dt = Ax + Bu</para>
        /// <para>y = Cx + Du</para>
        /// </summary>
        /// <param name="A">State matrix.</param>
        /// <param name="B">Input matrix.</param>
        /// <param name="C">Output matrix.</param>
        /// <param name="D">Feedthrough matrix.</param>
        public LTISystem(Matrix A, Matrix B, Matrix C, Matrix D)
        {
            Init(A, B, C, D);
        }

        /// <summary>
        /// Define a SISO LTI system from a transfer function.
        /// </summary>
        public LTISystem(TransferFunction transferFunction, CanonicalForm canonicalForm = CanonicalForm.ObservableCanonicalForm)
        {
            // http://lpsa.swarthmore.edu/Representations/SysRepTransformations/TF2SS.html

            Polynomial numerator = transferFunction.numerator;
            Polynomial denominator = transferFunction.denominator;

            int n = denominator.Order;

            if (denominator.Length < numerator.Length) {
                Debug.LogError("StateSpaceModel: Numerator shorter or equally long as denominator!");
                throw new System.Exception();
            }

            numerator.PadWithZeros(denominator.Length);

            Matrix A;
            Matrix B;
            Matrix C;

            float[,] vals;
            if (canonicalForm == CanonicalForm.ControllableCanonicalForm) {
                vals = new float[n, n];
                for (int row = 0; row < n; row++) {
                    for (int col = 0; col < n; col++) {
                        if (row == n - 1) { // Last row
                            vals[row, col] = -denominator[n - col];
                        } else {
                            if (col == row + 1) {
                                vals[row, col] = 1f;
                            } else {
                                vals[row, col] = 0f;
                            }
                        }
                    }
                }
                A = new Matrix(vals);

                vals = new float[n, 1];
                for (int i = 0; i < n; i++) {
                    if (i == n - 1) {
                        vals[i, 0] = 1f;
                    } else {
                        vals[i, 0] = 0f;
                    }
                }
                B = new Matrix(vals);

                vals = new float[1, n];
                for (int i = 0; i < n; i++) {
                    vals[0, i] = numerator[n - i] - denominator[n - i] * numerator[0];
                }
                C = new Matrix(vals);

            } else {//if (canonicalForm == CanonicalForm.ObservableCanonicalForm) {
                vals = new float[n, n];
                for (int row = 0; row < n; row++) {
                    for (int col = 0; col < n; col++) {
                        if (col == 0) { // First column
                            vals[row, col] = -denominator[row + 1];
                        } else {
                            if (col == row + 1) {
                                vals[row, col] = 1f;
                            } else {
                                vals[row, col] = 0f;
                            }
                        }
                    }
                }
                A = new Matrix(vals);

                vals = new float[n, 1];
                for (int i = 0; i < n; i++) {
                    vals[i, 0] = numerator[i + 1] - denominator[i + 1] * numerator[0];
                }
                B = new Matrix(vals);

                vals = new float[1, n];
                for (int i = 0; i < n; i++) {
                    if (i == 0) {
                        vals[0, i] = 1f;
                    } else {
                        vals[0, i] = 0f;
                    }
                }
                C = new Matrix(vals);
            }

            Matrix D = Matrix.One(1) * numerator[0];
            Init(A, B, C, D);
        }

        /// <summary>
        /// Initializes the system from matrices of a LTI system. Avoids redundant code.
        /// </summary>
        void Init(Matrix A, Matrix B, Matrix C, Matrix D)
        {
            int n = A.cols; // States
            int m = B.cols; // Inputs
            int p = C.rows; // Outputs

            this.x = Matrix.Zero(n, 1);

            if (A.rows != n || B.rows != n || C.cols != n || D.cols != m || D.rows != p) {
                Debug.LogError("StateSpaceModel: Matrix dimensions of at least one matrix are wrong!");
                throw new System.Exception();
            }

            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }



        /// <summary>
        /// Set the internal state of the system.
        /// </summary>
        public void SetState(Matrix x)
        {
            if (x.cols != 1 || x.rows != n) {
                Debug.LogError("State dimensions are wrong!");
                throw new System.Exception();
            }
            this.x = x;
        }

        /// <summary>
        /// Returns the TransferFunction of the system from one input to one output
        /// </summary>
        public TransferFunction TransferFunction()
        {
            // Difficulties: Inverse matrix calculation and symbolic calculations

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input vector</param>
        public Matrix Eval(Matrix u)
        {
            Matrix y;

            y = C * x + D * u;
            x = A * x + B * u;

            return y;
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input vector</param>
        public float[] Eval(float[] u)
        {
            return Eval(Matrix.FromFloatArray(u)).ToFloatArray();
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input</param>
        /// <param name="deltaTime">Time since last evaluation. Should be Time.fixedDeltaTime, if you evaluate every FixedUpdate.</param>
        /// <returns></returns>
        public float Eval(float u)
        {
            return Eval(Matrix.FromFloat(u)).ToFloat();
        }

        /// <summary>
        /// Make a copy of the LTISystem.
        /// </summary>
        public LTISystem Copy()
        {
            LTISystem sys = new LTISystem(A,B,C,D);
            sys.SetState(x);
            return sys;
        }

        /// <summary>
        /// Logs all important properties of the system.
        /// </summary>
        public void Log()
        {
            Debug.Log("n=" + n + ", m=" + m + ", p=" + p);
            Debug.Log("A=");
            A.Log();
            Debug.Log("B=");
            B.Log();
            Debug.Log("C=");
            C.Log();
            Debug.Log("D=");
            D.Log();
        }
    }

    public class SSSystem
    {
        StateFunction f;
        StateFunction g;

        /// <summary>
        /// The current state of the system. To set the state, use SetState().
        /// </summary>
        public Matrix x { get; private set; }
        Matrix dx;

        /// <summary>
        /// Differential equation solver algorithm.
        /// </summary>
        public Solver solver;

        public int n { get; private set; }

        

        /// <summary>
        /// <para>Define an arbitrary time invariant system.</para>
        /// <para>dx/dt = f(x,u)</para>
        /// <para>y = g(x,u)</para>
        /// </summary>
        /// <param name="f">State function.</param>
        /// <param name="g">Output function.</param>
        /// <param name="n">Number of states.</param>
        public SSSystem(StateFunction f, StateFunction g, int n)
        {
            this.f = f;
            this.g = g;

            this.n = n;

            this.x = Matrix.Zero(n, 1);

            solver = Solvers.Discrete;
        }

        /// <summary>
        /// Set the internal state of the system.
        /// </summary>
        public void SetState(Matrix x)
        {
            if (x.cols != 1 || x.rows != n) {
                Debug.LogError("State dimensions are wrong!");
                throw new System.Exception();
            }
            this.x = x;
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input vector</param>
        public Matrix Eval(Matrix u)
        {
            Matrix y;

            y = g(x, u);
            x = solver(x, u, f, Time.fixedDeltaTime);

            return y;
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input vector</param>
        public float[] Eval(float[] u)
        {
            return Eval(Matrix.FromFloatArray(u)).ToFloatArray();
        }

        /// <summary>
        /// Evaluates the input while returning the output.
        /// </summary>
        /// <param name="u">Input</param>
        public float Eval(float u)
        {
            return Eval(Matrix.FromFloat(u)).ToFloat();
        }

        /// <summary>
        /// Make a copy of the StateSpaceSystem.
        /// </summary>
        public SSSystem Copy()
        {
            SSSystem ssm = new SSSystem(f,g,n);
            ssm.SetState(x);
            return ssm;
        }
    }
	#endregion

	#region Controllers
	public class VelocityController2D {
		LTISystem G_wS_x;
		LTISystem G_Aw_x;
		LTISystem obs_controller_x;
		LTISystem G_wS_y;
		LTISystem G_Aw_y;
		LTISystem obs_controller_y;

		public VelocityController2D(Complex ffPoles, Complex fbPoles, Rigidbody2D rb)
		{
			Vector2 initialPosition = rb.position;
			float m = rb.mass;
			float T = Time.fixedDeltaTime;

			// G_wS
			TransferFunction tf = new TransferFunction(
				new float[] { 1f, 1f },
				new float[] { 1f, -2f * ffPoles.Real, Mathf.Pow(ffPoles.Magn, 2) },
				0.5f * (1f - 2f * ffPoles.Real + Mathf.Pow(ffPoles.Magn, 2))
			);
			G_wS_x = new LTISystem(tf);
			//G_wS_x.Log();

			// G_Aw
			tf = new TransferFunction(
				new float[] { 1f, -2f, 1f },
				new float[] { 1f, -2f * ffPoles.Real, Mathf.Pow(ffPoles.Magn, 2) },
				m / (T * T) * (1f - 2f * ffPoles.Real + Mathf.Pow(ffPoles.Magn, 2))
			);
			G_Aw_x = new LTISystem(tf);
			//G_Aw_x.Log();

			Matrix A = new Matrix(new float[,] {
				{ 1f, T },
				{ 0f, 1f}
			});
			Matrix B = new Matrix(new float[,] {
				{ T*T/(2*m) },
				{ T/m }
			});
			Matrix C = new Matrix(new float[,] {
				{ 1f, 0f }
			});
			Matrix k = new Matrix(new float[,] {
				{ m/(T*T)* (1f-2f*fbPoles.Real + Mathf.Pow(fbPoles.Magn,2)), m/(2*T)*(3f-2f*fbPoles.Real-(Mathf.Pow(fbPoles.Magn,2)))}
			});
			Matrix l = new Matrix(new float[,] {
				{ 2f },
				{ 1/T }
			});
			obs_controller_x = new LTISystem(A - B * k - l * C, l, k);

			G_wS_y = G_wS_x.Copy();
			G_Aw_y = G_Aw_x.Copy();
			obs_controller_y = obs_controller_x.Copy();

			// Set initial state of G_wS, so it does not do weird jumps in the beginning
			float factor = (1 - G_wS_x.A.vals[0, 0] - G_wS_x.B.vals[0, 0]) / G_wS_x.A.vals[0, 1];
			Matrix initialState = new Matrix(new float[,] {
				{ initialPosition.x },
				{ initialPosition.x * factor }
			});
			G_wS_x.SetState(initialState);

			initialState = new Matrix(new float[,] {
				{ initialPosition.y },
				{ initialPosition.y * factor }
			});
			G_wS_y.SetState(initialState);

			// Set initial state of G_Aw, so it does not do weird jumps in the beginning
			factor = (G_Aw_x.B.vals[0,0] + G_Aw_x.D.vals[0, 0] * (1 - G_Aw_x.A.vals[0,0])) / G_Aw_x.A.vals[0,1];
			initialState = new Matrix(new float[,] {
				{ -G_Aw_x.D.vals[0,0] * initialPosition.x },
				{ -factor * initialPosition.x }
			});
			G_Aw_x.SetState(initialState);

			initialState = new Matrix(new float[,] {
				{ -G_Aw_y.D.vals[0,0] * initialPosition.y },
				{ -factor * initialPosition.y }
			});
			G_Aw_y.SetState(initialState);
		}

		public Vector3 Eval(Vector2 posTarget, Vector2 posCurrent)
		{
			Vector2 w = posTarget;
			Vector2 y = posCurrent;

			Vector2 y_s = new Vector2 {
				x = G_wS_x.Eval(w.x),
				y = G_wS_y.Eval(w.y)
			};

			Vector2 e = y_s - y;

			Vector2 u_s = new Vector2 {
				x = G_Aw_x.Eval(w.x),
				y = G_Aw_y.Eval(w.y)
			};

			Vector2 u_r = new Vector2 {
				x = obs_controller_x.Eval(e.x),
				y = obs_controller_y.Eval(e.y)
			};

			Vector2 u_out = u_s + u_r;

			//Debug.Log("y: " + y + ", y_s: " + y_s + ", w: " + w + ", u_r: " + u_r);
			//Debug.Log("w: " + w + ", u_s: " + u_out);

			return new Vector3(u_out.x, u_out.y, 0f);
		}
	}

	// TODO: Steady state accuracy
	public class RotationController2D
	{
		LTISystem G_wS;
		LTISystem G_Aw;
		LTISystem obs_controller;
		float w_old;
		float y_old;

		public RotationController2D(Complex ffPoles, Complex fbPoles, Rigidbody2D rb) {
			float T = Time.fixedDeltaTime;
			float I = rb.inertia;

			// G_wS
			TransferFunction tf = new TransferFunction(
				new float[] { 1f, 1f },
				new float[] { 1f, -2f * ffPoles.Real, Mathf.Pow(ffPoles.Magn, 2) },
				0.5f * (1f - 2f * ffPoles.Real + Mathf.Pow(ffPoles.Magn, 2))
			);

			G_wS = new LTISystem(tf);
			/*
            tf.Log();
            G_wS.Log();
			*/
			// G_Aw
			tf = new TransferFunction(
				new float[] { 1f, -2f, 1f },
				new float[] { 1f, -2f * ffPoles.Real, Mathf.Pow(ffPoles.Magn, 2) },
				I / (T * T) * (1f - 2f * ffPoles.Real + Mathf.Pow(ffPoles.Magn, 2))
			);
			G_Aw = new LTISystem(tf);
			//tf.Log();
			//Debug.Log(I / (T * T) * (1f - 2f * ff_poles_real + Mathf.Pow(ff_poles_magn, 2)));
			//Debug.Log(I + ", " + T + ", " + ff_poles_real + ", " + ff_poles_magn);
			//G_Aw.Log();

			Matrix A = new Matrix(new float[,] {
				{ 1f, T },
				{ 0f, 1f}
			});
			Matrix B = new Matrix(new float[,] {
				{ T*T/(2*I) },
				{ T/I }
			});
			Matrix C = new Matrix(new float[,] {
				{ 1f, 0f }
			});
			Matrix k = new Matrix(new float[,] {
				{ I/(T*T)* (1f-2f*fbPoles.Real + Mathf.Pow(fbPoles.Magn,2)), I/(2*T)*(3f-2f*fbPoles.Real-(Mathf.Pow(fbPoles.Magn,2)))}
			});
			Matrix l = new Matrix(new float[,] {
				{ 2f },
				{ 1/T }
			});
			obs_controller = new LTISystem(A - B * k - l * C, l, k);
		}

		public float Eval(float rotTarget, float rotCurrent) {
			float w = rotTarget;
			float y = rotCurrent;

			w = Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Radians, w, w_old);
			y = Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Radians, y, y_old);

			float y_s = G_wS.Eval(w);
			float e = y_s - y;

			float u_s = G_Aw.Eval(w);
			float u_r = obs_controller.Eval(e);
			float u_out = u_s + u_r;

			w_old = w;
			y_old = y;

			return u_out;
		}
	}
	
    public class PID
    {
        /// <summary>
        /// Struct containing the P-, I-, and D-values of a PID Controller
        /// </summary>
        public struct values
        {
            public float p;
            public float i;
            public float d;
        }

        values val;

        Integrator i;
        Differentiator d;

        /// <summary>
        /// Creates a PID Controller.
        /// </summary>
        /// <param name="val">Struct containing the P-, I-, and D-values for the PID Controller</param>
        public PID(values val)
        {
            this.val = val;
            if (val.i != 0.0f) {
                i = new Integrator();
            }
            if (val.d != 0.0f) {
                d = new Differentiator();
            }
        }

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        public float Eval(float input)
        {
            float output = 0;


            output += val.p * input;

            if (i != null)
                output += val.i * i.Eval(input);

            if (d != null)
                output += val.d * d.Eval(input);

            return output;
        }
    }

    public class PIDVector3
    {
        PID x;
        PID y;
        PID z;

        /// <summary>
        /// <para>Creates three PID Controllers, which independendly act on each component of a Vector3.</para>
        /// <para>It is recommended to set I = 0 for simple position control tasks. Instead, add a force equal to the negative gravity.</para>
        /// </summary>
        /// <param name="val">Struct containing the P-, I-, and D-values for the three PID controllers. </param>
        public PIDVector3(PID.values val)
        {
            x = new PID(val);
            y = new PID(val);
            z = new PID(val);
        }

        /// <summary>
        /// Evaluates an input while returning the output.
        /// </summary>
        public Vector3 Eval(Vector3 input)
        {
            Vector3 result;
            result.x = x.Eval(input.x);
            result.y = y.Eval(input.y);
            result.z = z.Eval(input.z);
            return result;
        }
    }
	#endregion
}