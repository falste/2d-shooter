using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;

//Singleton pattern:
/*
public class Singleton {
    static Singleton instance_;

    public static Singleton Instance
    {
        get {
            if (instance_ == null) {
                instance_ = new Singleton();
            }

            return instance_;
        }
    }
}
*/


namespace Utility
{
	public static class EditorUtils {
		public static void ScriptField(Editor context) {
			SerializedObject serializedObject = context.serializedObject;
			serializedObject.Update();
			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
			GUI.enabled = true;
		}

		public static bool StaticClassFoldout(bool foldout, System.Type type, string name = null) {
			if (name == null) {
				name = ObjectNames.NicifyVariableName(type.Name);
			}
			
			foldout = EditorGUILayout.Foldout(foldout, name, true);
			if (foldout) {
				EditorGUI.indentLevel++;
				foreach (System.Reflection.FieldInfo field in type.GetFields()) {
					if (field.FieldType == typeof(bool)) {
						field.SetValue(null, EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(field.Name), (bool)field.GetValue(null)));
					} // Add more field types or use SerializedProperties instead.
				}
				EditorGUI.indentLevel--;
			}

			return foldout;
		}

		public static bool ScriptableObjectFoldout<T>(bool foldout, ref T so, ref Editor soEditor, string name = null) where T : ScriptableObject {
			if (name == null) {
				name = ObjectNames.NicifyVariableName(typeof(T).Name);
			}

			// Use rect to draw ObjectField and Foldout in the same spot
			Rect rect = EditorGUILayout.GetControlRect();
			so = (T)EditorGUI.ObjectField(rect, name, so, typeof(T), true);
			foldout = EditorGUI.Foldout(rect, foldout, (string)null, true);
			if (so != null) {
				if (foldout) {
					EditorGUI.indentLevel++;
					if (soEditor == null) {
						soEditor = Editor.CreateEditorWithContext(new T[] { so }, null);
					}
					soEditor.OnInspectorGUI();
					EditorGUI.indentLevel--;
				}

				// https://gamedev.stackexchange.com/questions/125698/how-to-edit-and-persist-serializable-assets-in-the-editor-window
				if (GUI.changed) {
					EditorUtility.SetDirty(so);
				}
			}

			return foldout;
		}
	}

    namespace StateMachine
    {
		public interface IStateMachineBlackboard {

		}

        public class StateMachine
        {
			public State State { get; private set; }
			public readonly IStateMachineBlackboard blackboard;
			bool forceState;

			public StateMachine (IStateMachineBlackboard blackboard) {
				this.blackboard = blackboard;
			}

			public void ForceState(bool force) {
				if (force) {
					Debug.Log("Forcing state to " + State.ToString());
				}

				forceState = force;
			}

            public void Transition(State newState)
            {
				if (forceState)
					return;
				if (State == newState)
					return;

				//Debug.Log("Transitioning to " + newState.ToString());
				if (State != null)
					State.OnExit();
				State = newState;
				State.OnEnter();
            }
			
			public void Update() {
				if (State != null)
					State.OnUpdate();
			}

			public void FixedUpdate() {
				if (State != null)
					State.OnFixedUpdate();
			}

			public void OnDrawGizmos() {
				if (State != null)
					State.OnDrawGizmos();
			}

			public void OnDrawGizmosSelected() {
				if (State != null)
					State.OnDrawGizmosSelected();
			}
        }

		public abstract class State
        {
			protected StateMachine sm;

			public State(StateMachine sm) {
				this.sm = sm;
			}

			public virtual void OnEnter() { }
			public virtual void OnUpdate() { }
			public virtual void OnFixedUpdate() { }
			public virtual void OnExit() { }
			public virtual void OnDrawGizmos() { }
			public virtual void OnDrawGizmosSelected() { }
		}
    }

    public static class General
    {
		[System.Obsolete]
		public static Vector2 V3toV2(Vector3 v3) {
			return new Vector2 {
				x = v3.x,
				y = v3.y
			};
		}

		// Draws lines from p1 to p2 to p3 to p4
		public static void DrawRectGizmo(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}

        public static readonly Mesh QuadMesh = new Mesh {
            vertices = new Vector3[] {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f)
            },
            triangles = new int[] { 0, 2, 1, 2, 3, 1 },
            normals = new Vector3[] {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            }
        };
    }

    public static class Calc
    {       
        public static class Angles
        {
			public static float Rad2Deg = 180 / Mathf.PI;
			public static float Deg2Rad = Mathf.PI / 180;

            public enum AngleMeasure { Degrees, Radians };

            /// <summary>
            /// Returns the period of the given angle measure
            /// </summary>
            public static float Period(AngleMeasure angleMeasure)
            {
                switch (angleMeasure){
                    case AngleMeasure.Degrees:
                        return 360f;
                    case AngleMeasure.Radians:
                        return Mathf.PI * 2;
                    default:
                        Debug.LogError("AngleMeasure not recognized. This should never happen!");
                        return 0f;
                }
            }

            public static float HalfPeriod(AngleMeasure angleMeasure)
            {
                switch (angleMeasure) {
                    case AngleMeasure.Degrees:
                        return 180f;
                    case AngleMeasure.Radians:
                        return Mathf.PI;
                    default:
                        Debug.LogError("AngleMeasure not recognized. This should never happen!");
                        return 0f;
                }
            }

            /// <summary>
            /// Returns a conversion factor for the conversion from one angle measure to another.
            /// </summary>
			[System.Obsolete]
            public static float MeasureConverter(AngleMeasure from, AngleMeasure to)
            {
                if (from == to) {
                    return 1f;
                }

                return Period(to) / Period(from);
            }

            /// <summary>
            /// <para>Returns the quickest angle that can be reached with the current velocity, that is equivalent to the target angle.
            /// This is done by calculating the closest position the object can stop at using its maxAcceleration and then getting the closest periodic equivalent of the target position to the just calculated stopping position.</para>
            /// <para>!!! Angles have to be provided in degrees, angular velocity in radians/second and maxAcceleration in radians/second^2, as is the standard in Unity !!!</para>
            /// </summary>
            /// <param name="currentAngle">Current angle of the object in degrees (standard for Transform.rotation).</param>
            /// <param name="targetAngle">The angle that shall be reached eventually in degrees (standard for Transform.rotation).</param>
            /// <param name="angVelocity">Current angular velocity in radians/second (standard for Rigidbody.angularVelocity).</param>
            /// <param name="maxAcceleration">Maximal acceleration of the object in radians/second^2 (standard for Rigidbody.AddForce() with ForceMode.acceleration).</param>
            public static float BestStoppingTargetAngle(float targetAngle, float currentAngle, float angVelocity, float maxAcceleration)
            {
                float stopAngle = currentAngle + Mathf.Abs(angVelocity) / maxAcceleration * 0.5f * angVelocity; // angular velocity is measured in rad/s in unity

                return ClosestPeriodicEquivalent(AngleMeasure.Radians, targetAngle, stopAngle);
            }

            /// <summary>
            /// Returns the closest value to reference, that is equivalent to angle due to its periodicity.
            /// </summary>
            public static float ClosestPeriodicEquivalent(AngleMeasure angleMeasure, float angle, float reference = 0.0f)
            {
                float period = Period(angleMeasure);

                while (Mathf.Abs(angle + period - reference) < Mathf.Abs(angle - reference)) {
                    angle += period;
                }
                while (Mathf.Abs(angle - period - reference) < Mathf.Abs(angle - reference)) {
                    angle -= period;
                }
                return angle;
            }

			public static float ClockwiseAngle(Vector2 v1, Vector2 v2) {
				float sign = Mathf.Sign(v1.x * v2.y - v1.y * v2.x);
				return Vector2.Angle(v1, v2) * sign;
			}
        }

		public static int V2IntOneNorm(Vector2Int v) {
			return Mathf.Abs(v.x) + Mathf.Abs(v.y);
		}

        public static float Derivate(float newValue, float oldValue, float dT)
        {
            return (newValue - oldValue) / dT;
        }

        // See https://groups.google.com/forum/#!topic/scala-user/rC5_YBpu44c
        public static int FlooredDivision(int a, int b)
        {
            int q = a / b;
            if (q * b == a || Math.Sign(a) == Math.Sign(b)) {
                return q;
            } else {
                return q - 1;
            }
        }

        public static Vector3 ClampVector3(Vector3 vec, float min, float max)
        {
            vec.x = Mathf.Clamp(vec.x, min, max);
            vec.y = Mathf.Clamp(vec.y, min, max);
            vec.z = Mathf.Clamp(vec.z, min, max);
            return vec;
        }

        public static Vector2 ClampVector2(Vector2 vec, float min, float max)
        {
            vec.x = Mathf.Clamp(vec.x, min, max);
            vec.y = Mathf.Clamp(vec.y, min, max);
            return vec;
        }

        public static Vector3 ClampVector3Magnitude(Vector3 vec, float min, float max)
        {
            if (vec.sqrMagnitude < min * min) {
                return vec.normalized * min;
            } else if (vec.sqrMagnitude > max * max) {
                return vec.normalized * max;
            } else {
                return vec;
            }
        }

        public static Vector2 ClampVector2Magnitude(Vector2 vec, float min, float max)
        {
            if (vec.sqrMagnitude < min * min) {
                return vec.normalized * min;
            } else if (vec.sqrMagnitude > max * max) {
                return vec.normalized * max;
            } else {
                return vec;
            }
        }

        public static float GaussianDensityDistribution(float x, float sigma, float mu)
        {
            return 1 / Mathf.Sqrt(2 * Mathf.PI * sigma * sigma) * Mathf.Exp(-Mathf.Pow(x - mu, 2) / (2 * sigma * sigma));
        }

		public static float InterceptTime(Vector2 relativeTargetPosition, Vector2 relativeTargetVelocity, float projectileSpeed) {
			// http://officialtwelve.blogspot.com/2015/08/projectile-interception.html

			float angle = Angles.ClockwiseAngle(relativeTargetVelocity, -relativeTargetPosition);

			float a = relativeTargetVelocity.sqrMagnitude - Mathf.Pow(projectileSpeed, 2);
			float b = relativeTargetVelocity.magnitude * (-2 * relativeTargetPosition.magnitude * Mathf.Cos(angle * Mathf.Deg2Rad));
			float c = relativeTargetPosition.sqrMagnitude;

			float determinant = b * b - 4 * a * c;

			if (determinant < 0 || a == 0f) {
				return 0f; // No possible intercept! a == 0f check prevents division by 0
			}

			float t1 = (-b - Mathf.Sqrt(determinant)) / (2 * a);
			if (t1 > 0)
				return t1;

			float t2 = (-b + Mathf.Sqrt(determinant)) / (2 * a);
			return t2;
		}

		public static Vector2 RelativeInterceptPoint(Vector2 relativeTargetPosition, Vector2 relativeTargetVelocity, float interceptTime) {
			return relativeTargetPosition + relativeTargetVelocity * interceptTime;
		}
    }

	public class Complex {
		public enum Notation { RealImag, MagnPhase }
		public float Real { get; private set; }
		public float Imag { get; private set; }
		public float Magn { get; private set; }
		public float Phase { get; private set; }

		/// <summary>
		/// Creates a complex number
		/// </summary>
		/// <param name="a">Real part or magnitude</param>
		/// <param name="b">Imaginary part or phase</param>
		public Complex(float a, float b, Notation notation) {
			if (notation == Notation.RealImag) {
				Real = a;
				Imag = b;
				Magn = Mathf.Sqrt(Real * Real + Imag * Imag);
				if (b >= 0) {
					Phase = Mathf.Acos(Real / Magn);
				} else {
					Phase = -Mathf.Acos(Real / Magn);
				}
			} else {
				Magn = a;
				Phase = b;
				Real = Magn * Mathf.Cos(Phase);
				Imag = Magn * Mathf.Sin(Phase);
			}
		}
	}

    public class Matrix
    {
        public int rows { get { return vals.GetLength(0); } }
        public int cols { get { return vals.GetLength(1); } }

        public float[,] vals { get; private set; }

        public Matrix(float[,] vals)
        {
            this.vals = vals;
        }

        public static Matrix MatrixFromString(String input)
        {
            char colChar = ';';
            char rowChar = ' ';

            String[] rows = input.Split(colChar);
            String[] values = rows[0].Split(rowChar);
            int colLength = rows.Length;
            int rowLength = values.Length;
            float[,] vals = new float[colLength, rowLength];

            rows = input.Split(colChar);
            for (int i = 0; i < rows.Length; i++) {
                if (rows.Length != colLength) {
                    Debug.LogError("MatrixFromString: Input is not rectangular");
                }
                values = rows[i].Split(rowChar);
                for (int j = 0; j < values.Length; j++) {
                    vals[i, j] = float.Parse(values[j]);
                }
            }
            return new Matrix(vals);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.cols != b.rows) {
                Debug.LogError("Multiply: Can't multiply these matrices! Wrong dimensions!");
                return null;
            }
            if (a.cols < 1 || b.cols < 1 || a.rows < 1 ||b.rows < 1) {
                Debug.LogError("Matrix dimensions must be greater than 0!");
                return null;
            }


            float[,] vals = new float[a.rows, b.cols];
            float res;
            for (int i = 0; i < a.rows; i++) {
                for (int j = 0; j < b.cols; j++) {
                    res = 0;

                    for (int k = 0; k < a.cols; k++) {
                        res += a.vals[i, k] * b.vals[k, j];
                    }

                    vals[i, j] = res;
                }
            }
            return new Matrix(vals);
        }

        public static Matrix operator *(Matrix a, float b)
        {
            float[,] vals = new float[a.rows, a.cols];

            for (int i = 0; i < a.rows; i++) {
                for (int j = 0; j < a.cols; j++) {
                    vals[i, j] = a.vals[i, j] * b;
                }
            }

            return new Matrix(vals);
        }

        public static Matrix operator *(float b, Matrix a)
        {
            return a * b;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.cols != b.cols || a.rows != b.rows) {
                Debug.LogError("Add: Can't add these matrices! Wrong dimensions! Dimensions: " + a.rows + "x" + a.cols + ", " + b.rows + "x" + b.cols);
                return null;
            }

            float[,] vals = new float[a.rows, a.cols];
            for (int i = 0; i < a.rows; i++) {
                for (int j = 0; j < a.cols; j++) {
                    vals[i, j] = a.vals[i, j] + b.vals[i, j];
                }
            }

            return new Matrix(vals);
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.cols != b.cols || a.rows != b.rows) {
                Debug.LogError("Add: Can't add these matrices! Wrong dimensions! Dimensions: " + a.rows + "x" + a.cols + ", " + b.rows + "x" + b.cols);
                return null;
            }

            float[,] vals = new float[a.rows, a.cols];
            for (int i = 0; i < a.rows; i++) {
                for (int j = 0; j < a.cols; j++) {
                    vals[i, j] = a.vals[i, j] - b.vals[i, j];
                }
            }

            return new Matrix(vals);
        }

        public static Matrix operator -(Matrix a)
        {
            for (int i = 0; i < a.rows; i++) {
                for (int j = 0; j < a.cols; j++) {
                    a.vals[i, j] = -a.vals[i, j];
                }
            }

            return a;
        }

        public static Matrix Exp(Matrix A, int n)
        {
            if (A.rows != A.cols) {
                Debug.LogError("Matrix must be quadratic!");
                return null;
            }

            if (n == 0) {
                return Identity(A.rows);
            }

            Matrix res = A;
            for (int i = 1; i < n; i++) {
                res *= A;
            }

            return res;
        }

        public static Matrix CombineH(Matrix A, Matrix B)
        {
            if (A.rows != B.rows) {
                Debug.LogError("Both matrices must have the same number of rows!");
                return null;
            }

            float[,] res = new float[A.rows, A.cols + B.cols];

            for (int i = 0; i < A.rows; i++) {
                for (int j = 0; j < A.cols; j++) {
                    res[i, j] = A.vals[i, j];
                }
            }

            for (int i = 0; i < B.rows; i++) {
                for (int j = 0; j < B.cols; j++) {
                    res[i, j + A.cols] = B.vals[i, j];
                }
            }

            return new Matrix(res);
        }

        public static Matrix CombineV(Matrix A, Matrix B)
        {
            if (A.cols != B.cols) {
                Debug.LogError("Both matrices must have the same number of cols!");
                return null;
            }

            float[,] res = new float[A.rows + B.rows, A.cols];

            for (int i = 0; i < A.rows; i++) {
                for (int j = 0; j < A.cols; j++) {
                    res[i, j] = A.vals[i, j];
                }
            }

            for (int i = 0; i < B.rows; i++) {
                for (int j = 0; j < B.cols; j++) {
                    res[i + A.rows, j] = B.vals[i, j];
                }
            }

            return new Matrix(res);
        }

        public static Matrix Zero(int rows, int cols)
        {
            return new Matrix(new float[rows, cols]);
        }

        public static Matrix Zero(int dim)
        {
            return new Matrix(new float[dim, dim]);
        }

        public static Matrix One(int rows, int cols)
        {
            float[,] res = new float[rows, cols];

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    res[i, j] = 1f;
                }
            }

            return new Matrix(res);
        }

        public static Matrix One(int dim)
        {
            return One(dim, dim);
        }

        public static Matrix Identity(int dim)
        {
            float[,] vals = new float[dim, dim];
            for (int i = 0; i < dim; i++) {
                vals[i, i] = 1;
            }
            return new Matrix(vals);
        }

        public static Matrix Diag(params float[] values)
        {
            int dim = values.Length;

            float[,] vals = new float[dim, dim];
            for (int i = 0; i < dim; i++) {
                vals[i, i] = values[i];
            }
            return new Matrix(vals);
        }

        public bool IsUpperTriangle()
        {
            if (rows != cols) {
                return false;
            }

            for (int i = 0; i < cols; i++) {
                for (int j = 0; j < i; j++) {
                    if (vals[i, j] != 0f) {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsLowerTriangle()
        {
            if (rows != cols) {
                return false;
            }

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < i; j++) {
                    if (vals[j, i] != 0f) {
                        return false;
                    }
                }
            }

            return true;
        }

        public float ToFloat()
        {
            if (rows == 1 && cols == 1) {
                return vals[0, 0];
            } else {
                Debug.LogError("ToFloat: Matrix does not have exactly 1 element!");
                return 0.0f;
            }
        }

        public Vector2 ToVector2()
        {
            if (rows == 2 && cols == 1) {
                return new Vector2(vals[0, 0], vals[1, 0]);
            } else {
                Debug.LogError("ToVector2: Matrix does not have correct dimensions!");
                return Vector2.zero;
            }
        }

        public Vector3 ToVector3()
        {
            if (rows == 3 && cols == 1) {
                return new Vector3(vals[0, 0], vals[1, 0], vals[2, 0]);
            } else {
                Debug.LogError("ToVector3: Matrix does not have correct dimensions!");
                return Vector3.zero;
            }
        }

        public float[] ToFloatArray()
        {
            if (cols != 1) {
                Debug.LogError("ToFloatArray: Matrix does not have correct dimensions!");
                return new float[0];
            }

            float[] res = new float[rows];
            for (int i = 0; i < rows; i++) {
                res[i] = vals[i, 0];
            }
            return res;
        }

        public static Matrix FromFloat(float num)
        {
            float[,] vals = new float[1, 1];
            vals[0, 0] = num;
            return new Matrix(vals);
        }

        public static Matrix FromVector2(Vector2 vec)
        {
            float[,] vals = new float[2, 1];
            vals[0, 0] = vec.x;
            vals[1, 0] = vec.y;
            return new Matrix(vals);
        }

        public static Matrix FromVector3(Vector3 vec)
        {
            float[,] vals = new float[3, 1];
            vals[0, 0] = vec.x;
            vals[1, 0] = vec.y;
            vals[2, 0] = vec.z;
            return new Matrix(vals);
        }

        public static Matrix FromFloatArray(float[] array)
        {
            float[,] vals = new float[array.Length, 1];
            for (int i = 0; i < array.Length; i++) {
                vals[i, 0] = array[i];
            }
            return new Matrix(vals);
        }

        public override String ToString()
        {
            String s = "";
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    s += vals[i, j].ToString("0.00");

                    if (j != cols - 1) {
                        s += ", ";
                    }
                }

                if (i != rows - 1) {
                    s += " | ";
                }
            }

            return s;
        }

        public void Log()
        {
            String s = "";
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {
                    s += vals[i, j].ToString("0.00");

                    if (j != cols - 1) {
                        s += ", ";
                    }
                }
                Debug.Log(s);
                s = "";
            }
        }
    }

    public class Polynomial
    {
        public float[] coefficients;
        public int Length { get {
                return coefficients.Length;
            }
        }
        public int Order { get {
                return coefficients.Length - 1;
            }
        }

        public float this[int index]
        {
            get {
                return coefficients[index];
            }
            set {
                coefficients[index] = value;
            }
        }

        /// <summary>
        /// Create a new polynomial.
        /// </summary>
        /// <param name="coefficients">Coefficients starting from the highest order monomial.</param>
        public Polynomial(float[] coefficients)
        {
            this.coefficients = coefficients;
        }

        public Polynomial PadWithZeros(int targetLength)
        {
            int diff = targetLength - Length;

            if (diff < 0) {
                Debug.LogError("Trying to pad polynomial, but its order is bigger than the targeted order.");
            }

            float[] newPol = new float[targetLength];
            for (int i = 0; i < targetLength; i++) {
                if (i < targetLength - Length) {
                    newPol[i] = 0f;
                } else {
                    newPol[i] = this[i - (targetLength - Length)];
                }
            }

            this.coefficients = newPol;
            return this;
        }

        public Polynomial RemoveLeadingZeros()
        {
            int nonZeroIndex = 0;
            while (this[nonZeroIndex] == 0f && nonZeroIndex < Length - 1) {
                nonZeroIndex++;
            }

            float[] newPol = new float[Length - nonZeroIndex];
            for (int i = nonZeroIndex; i < Length; i++) {
                newPol[i - nonZeroIndex] = this[i];
            }
            this.coefficients = newPol;
            return this;
        }

        public override String ToString()
        {
            String str = "";

            for (int i = 0; i < Length; i++) {
                if (i == 0) {
                    str += this[i].ToString("0.##;-#.##") + "z^" + (Order - i);
                } else {
                    str += " " + this[i].ToString("+ 0.##;- #.##") + "z^" + (Order - i);
                }
            }
            return str;
        }

        public void Log()
        {
            Debug.Log(ToString());
        }

        public static Polynomial operator *(float a, Polynomial b)
        {
            for (int i = 0; i < b.Length; i++) {
                b[i] *= a;
            }
            return b;
        }

        public static Polynomial operator *(Polynomial a, float b)
        {
            return b * a;
        }

        // This is not the optimal algorithm
        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            float[] newPol = new float[a.Order + b.Order + 1];

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    newPol[i + j] += a[i] * b[j];
                }
            }

            return new Polynomial(newPol);
        }

        public Polynomial Copy()
        {
            return new Polynomial(coefficients);
        }
    }

    public class TransferFunction
    {
        public Polynomial numerator;
        public Polynomial denominator;

        public TransferFunction Inverse { get {
                return new TransferFunction(denominator, numerator);
            }
        }

        public TransferFunction(Polynomial numerator, Polynomial denominator, float factor = 1f)
        {
            Init(numerator, denominator, factor);
        }

        public TransferFunction(float[] numerator, float[] denominator, float factor = 1f)
        {
            Init(new Polynomial(numerator), new Polynomial(denominator), factor);
        }



        void Init(Polynomial numerator, Polynomial denominator, float factor = 1f)
        {
            if (factor != 1f) {
                for (int i = 0; i < numerator.Length; i++) {
                    numerator[i] *= factor;
                }
            }

            float d = denominator[0];
            if (d != 1f) {
                for (int i = 0; i < numerator.Length; i++) {
                    numerator[i] /= d;
                }
                for (int i = 0; i < denominator.Length; i++) {
                    denominator[i] /= d;
                }
            }


            this.numerator = numerator;
            this.denominator = denominator;
        }

        public void Log()
        {
            string numString = numerator.ToString();
            string denString = denominator.ToString();
            string div = "";

            int length = numString.Length > denString.Length ? numString.Length : denString.Length;

            for (int i = 0; i < length * 1.3f; i++) {
                div += "-";
            }

            string space = "";
            for (int i = 0; i < (length - numString.Length) / 2; i++) {
                space += " ";
            }
            numString = space + numString;

            space = "";
            for (int i = 0; i < (length - denString.Length) / 2; i++) {
                space += " ";
            }
            denString = space + denString;

            Debug.Log(numString + "\n" + div + "\n" + denString);
        }

        public TransferFunction Copy()
        {
            return new TransferFunction(numerator, denominator);
        }



        public static TransferFunction operator *(float a, TransferFunction b)
        {
            return new TransferFunction(a * b.numerator, b.denominator);
        }

        public static TransferFunction operator *(TransferFunction b, float a)
        {
            return new TransferFunction(a * b.numerator, b.denominator);
        }

        public static TransferFunction operator *(TransferFunction a, TransferFunction b)
        {
            return new TransferFunction(a.numerator * b.numerator, a.denominator * b.denominator);
        }
    }

    public static class Epoch
    {
        public static int GetSeconds()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //DateTime epochStart = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        }

        public static int GetSecondsSince2018()
        {
            //DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime epochStart = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        }
    }

    public static class Log
    {
        public static void LogArray<T>(T[] o)
        {
            string str = o[0].ToString();
            for (int i = 1; i < o.Length; i++) {
                str += ", " + o[i].ToString();
            }
            Debug.Log(str);
        }

        public static void LogFloatArray(float[] o)
        {
            string str = o[0].ToString("0.00");
            for (int i = 1; i < o.Length; i++) {
                str += ", " + o[i].ToString("0.00");
            }
            Debug.Log(str);
        }

        public static void LogArrayIndexed<T>(T[] o)
        {
            string str = "0: " + o[0].ToString();
            for (int i = 1; i < o.Length; i++) {
                str += " | " + i + ": " + o[i].ToString();
            }
            Debug.Log(str);
        }

		public static void LogArrayToCsv<T>(T[] o, string path) {
			string str = o[0].ToString();
			for (int i = 1; i < o.Length; i++) {
				str += "," + o[i].ToString();
			}

			StreamWriter writer = new StreamWriter(path, false);
			writer.WriteLine(str);
			writer.Close();
		}

		public class LogToCsv {
			String filePath;
			float[] values;
			int logIndex;

			public LogToCsv(string filePath, float duration = 10f) {
				values = new float[Mathf.RoundToInt(duration / Time.fixedDeltaTime)+1];
				logIndex = 0;
				this.filePath = filePath;
				Debug.Log("Starting log for " + filePath);
			}

			/// <summary>
			/// Use in FixedUpdate!
			/// </summary>
			public void Log(float variable) {
				if (logIndex < values.Length) {
					values[logIndex] = variable;
					logIndex++;
				} else if(logIndex == values.Length) {
					logIndex++;
					Debug.Log("Finished logging! Saving to "+filePath);

					string str = values[0].ToString();
					for (int i = 1; i < values.Length; i++) {
						str += "," + values[i].ToString();
					}

					StreamWriter writer = new StreamWriter(filePath, false);
					writer.WriteLine(Time.fixedDeltaTime.ToString());
					writer.WriteLine(str);
					writer.Close();
				}
			}
		}
    }

    public static class Hashing
    {
        // See https://forum.unity.com/threads/hash-function-for-game.452779/
        public static string SHA1SUM2(string str)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(str);
            var sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            return System.BitConverter.ToString(sha.ComputeHash(bytes));
        }

        public static string SHA256(string str)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
            foreach (byte bit in crypto) {
                hash.Append(bit.ToString("x2"));
            }
            return hash.ToString().ToLower();
        }

        public static string MD5(string str)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(str);
            var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return System.BitConverter.ToString(sha.ComputeHash(bytes));
        }
    }

	public static class Noise {
		// From:
		// https://flafla2.github.io/2014/08/09/perlinnoise.html
		// https://gist.github.com/Flafla2/f0260a861be0ebdeef76

		static int repeat = -1;

		public static float PerlinNoise3DOctaves(float x, float y, float z, int octaves, float persistence) {
			float total = 0;
			float frequency = 1;
			float amplitude = 1;
			float maxValue = 0;            // Used for normalizing result to 0.0 - 1.0
			for (int i = 0; i < octaves; i++) {
				total += PerlinNoise3D(x * frequency, y * frequency, z * frequency) * amplitude;

				maxValue += amplitude;

				amplitude *= persistence;
				frequency *= 2;
			}

			return total / maxValue;
		}

		static readonly int[] p = { 151,160,137,91,90,15,					// Hash lookup table as defined by Ken Perlin.  This is a randomly
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,	// arranged array of all numbers from 0-255 inclusive.
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
		};

		public static float PerlinNoise3D(float x, float y, float z) {
			if (repeat > 0) {                                   // If we have any repeat on, change the coordinates to their "local" repetitions
				x = x % repeat;
				y = y % repeat;
				z = z % repeat;
			}

			int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
			int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
			int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
			float xf = x - (int)x;                             // We also fade the location to smooth the result.
			float yf = y - (int)y;
			float zf = z - (int)z;
			float u = Fade(xf);
			float v = Fade(yf);
			float w = Fade(zf);

			int aaa, aba, aab, abb, baa, bba, bab, bbb;
			aaa = p[p[p[xi] + yi] + zi];
			aba = p[p[p[xi] + Inc(yi)] + zi];
			aab = p[p[p[xi] + yi] + Inc(zi)];
			abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
			baa = p[p[p[Inc(xi)] + yi] + zi];
			bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
			bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
			bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];

			float x1, x2, y1, y2;
			x1 = Lerp(Grad(aaa, xf, yf, zf),                // The gradient function calculates the dot product between a pseudorandom
						Grad(baa, xf - 1, yf, zf),              // gradient vector and the vector from the input coordinate to the 8
						u);                                     // surrounding points in its unit cube.
			x2 = Lerp(Grad(aba, xf, yf - 1, zf),                // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
						Grad(bba, xf - 1, yf - 1, zf),              // values we made earlier.
						  u);
			y1 = Lerp(x1, x2, v);

			x1 = Lerp(Grad(aab, xf, yf, zf - 1),
						Grad(bab, xf - 1, yf, zf - 1),
						u);
			x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1),
						  Grad(bbb, xf - 1, yf - 1, zf - 1),
						  u);
			y2 = Lerp(x1, x2, v);

			return Lerp(y1, y2, w);                       // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
		}

		static int Inc(int num) {
			num++;
			if (repeat > 0)
				num %= repeat;

			return num;
		}

		static float Grad(int hash, float x, float y, float z) {
			int h = hash & 15;                                  // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
			float u = h < 8 /* 0b1000 */ ? x : y;              // If the most significant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

			float v;                                           // In Ken Perlin's original implementation this was another conditional operator (?:).  I
																// expanded it for readability.

			if (h < 4 /* 0b0100 */)                             // If the first and second significant bits are 0 set v = y
				v = y;
			else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/)// If the first and second significant bits are 1 set v = x
				v = x;
			else                                                // If the first and second significant bits are not equal (0/1, 1/0) set v = z
				v = z;

			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
		}

		static float Fade(float t) {
			// Fade function as defined by Ken Perlin.  This eases coordinate values
			// so that they will "ease" towards integral values.  This ends up smoothing
			// the final output.
			return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
		}

		static float Lerp(float a, float b, float x) {
			return a + x * (b - a);
		}
	}
}
 