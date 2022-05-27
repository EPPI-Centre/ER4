export class StatFunctions {

	public static qnorm(p: number, upper: boolean): number {
		/* Reference:
		   J. D. Beasley and S. G. Springer 
		   Algorithm AS 111: "The Percentage Points of the Normal Distribution"
		   Applied Statistics
		*/
		//if(p<0 || p>1)
		//			throw new IllegalArgumentException("Illegal argument "+p+" for qnorm(p).");
		let split: number = 0.42;
		let a0: number = 2.50662823884;
		let a1: number = -18.61500062529;
		let a2: number = 41.39119773534;
		let a3: number = -25.44106049637;
		let b1: number = -8.47351093090;
		let b2: number = 23.08336743743;
		let b3: number = -21.06224101826;
		let b4: number = 3.13082909833;
		let c0: number = -2.78718931138;
		let c1: number = -2.29796479134;
		let c2: number = 4.85014127135;
		let c3: number = 2.32121276858;
		let d1: number = 3.54388924762;
		let d2: number = 1.63706781897;
		let q: number = p - 0.5;
		let r: number, ppnd: number;
		if (Math.abs(q) <= split) {
			r = q * q;
			ppnd = q * (((a3 * r + a2) * r + a1) * r + a0) / ((((b4 * r + b3) * r + b2) * r + b1) * r + 1);
		}
		else {
			r = p;
			if (q > 0) r = 1 - p;
			if (r > 0) {
				r = Math.sqrt(-Math.log(r));
				ppnd = (((c3 * r + c2) * r + c1) * r + c0) / ((d2 * r + d1) * r + 1);
				if (q < 0) ppnd = -ppnd;
			}
			else {
				ppnd = 0;
			}
		}
		if (upper) ppnd = 1 - ppnd;
		return (ppnd);
	}


	public static pnorm(z: number, upper: boolean): number {
		// Reference:
		//  I. D. Hill 
		//  Algorithm AS 66: "The Normal Integral"
		//  Applied Statistics
		let ltone: number = 7.0,
			utzero = 18.66,
			con = 1.28,
			a1 = 0.398942280444,
			a2 = 0.399903438504,
			a3 = 5.75885480458,
			a4 = 29.8213557808,
			a5 = 2.62433121679,
			a6 = 48.6959930692,
			a7 = 5.92885724438,
			b1 = 0.398942280385,
			b2 = 3.8052e-8,
			b3 = 1.00000615302,
			b4 = 3.98064794e-4,
			b5 = 1.986153813664,
			b6 = 0.151679116635,
			b7 = 5.29330324926,
			b8 = 4.8385912808,
			b9 = 15.1508972451,
			b10 = 0.742380924027,
			b11 = 30.789933034,
			b12 = 3.99019417011;
		let y: number, alnorm: number;

		if (z < 0) {
			upper = !upper;
			z = -z;
		}
		if (z <= ltone || upper && z <= utzero) {
			y = 0.5 * z * z;
			if (z > con) {
				alnorm = b1 * Math.exp(-y) / (z - b2 + b3 / (z + b4 + b5 / (z - b6 + b7 / (z + b8 - b9 / (z + b10 + b11 / (z + b12))))));
			}
			else {
				alnorm = 0.5 - z * (a1 - a2 * y / (y + a3 - a4 / (y + a5 + a6 / (y + a7))));
			}
		}
		else {
			alnorm = 0;
		}
		if (!upper) alnorm = 1 - alnorm;
		return (alnorm);
	}


	public static qt(p: number, ndf: number, lower_tail: boolean) {
		// Algorithm 396: Student's t-quantiles by
		// G.W. Hill CACM 13(10), 619-620, October 1970
		//	if(p<=0 || p>=1 || ndf<1) 
		//		throw new IllegalArgumentException("Invalid p or df in call to qt(double,double,boolean).");
		let eps: number = 1e-12;
		let M_PI_2: number = 1.570796326794896619231321691640; // pi/2
		let neg: boolean;
		let P: number, q: number, prob: number, a: number, b: number, c: number, d: number, y: number, x: number;
		if ((lower_tail && p > 0.5) || (!lower_tail && p < 0.5)) {
			neg = false;
			P = 2 * (lower_tail ? (1 - p) : p);
		}
		else {
			neg = true;
			P = 2 * (lower_tail ? p : (1 - p));
		}

		if (Math.abs(ndf - 2) < eps) {   /* df ~= 2 */
			q = Math.sqrt(2 / (P * (2 - P)) - 2);
		}
		else if (ndf < 1 + eps) {   /* df ~= 1 */
			prob = P * M_PI_2;
			q = Math.cos(prob) / Math.sin(prob);
		}
		else {      /*-- usual case;  including, e.g.,  df = 1.1 */
			a = 1 / (ndf - 0.5);
			b = 48 / (a * a);
			c = ((20700 * a / b - 98) * a - 16) * a + 96.36;
			d = ((94.5 / (b + c) - 3) / b + 1) * Math.sqrt(a * M_PI_2) * ndf;
			y = Math.pow(d * P, 2 / ndf);
			if (y > 0.05 + a) {
				/* Asymptotic inverse expansion about normal */
				x = StatFunctions.qnorm(0.5 * P, false);
				y = x * x;
				if (ndf < 5)
					c += 0.3 * (ndf - 4.5) * (x + 0.6);
				c = (((0.05 * d * x - 5) * x - 7) * x - 2) * x + b + c;
				y = (((((0.4 * y + 6.3) * y + 36) * y + 94.5) / c - y - 3) / b + 1) * x;
				y = a * y * y;
				if (y > 0.002)/* FIXME: This cutoff is machine-preCIsion dependent*/
					y = Math.exp(y) - 1;
				else { /* Taylor of    e^y -1 : */
					y = (0.5 * y + 1) * y;
				}
			}
			else {
				y = ((1 / (((ndf + 6) / (ndf * y) - 0.089 * d - 0.822)
					* (ndf + 2) * 3) + 0.5 / (ndf + 4))
					* y - 1) * (ndf + 1) / (ndf + 2) + 1 / y;
			}
			q = Math.sqrt(ndf * y);
		}
		if (neg) q = -q;
		return q;
	}

	public static pt(t: number, df: number): number {
		// ALGORITHM AS 3  APPL. STATIST. (1968) VOL.17, P.189
		// Computes P(T<t)
		let a: number, b: number, idf: number, im2: number,
			ioe: number, s: number, c: number, ks: number, fk: number, k: number;

		let g1: number = 0.3183098862;// =1/pi;

		idf = df;
		a = t / Math.sqrt(idf);
		b = idf / (idf + t * t);
		im2 = df - 2;
		ioe = idf % 2;
		s = 1;
		c = 1;
		idf = 1;
		ks = 2 + ioe;
		fk = ks;
		if (im2 >= 2) {
			for (k = ks; k <= im2; k += 2) {
				c = c * b * (fk - 1) / fk;
				s += c;
				if (s != idf) {
					idf = s;
					fk += 2;
				}
			}
		}
		if (ioe != 1)
			return 0.5 + 0.5 * a * Math.sqrt(b) * s;
		if (df == 1)
			s = 0;
		return 0.5 + (a * b * s + Math.atan(a)) * g1;
	}

	public static pchisq(q: number, df: number): number {
		// Posten, H. (1989) American StatistiCIan 43 p. 261-265
		let df2: number = df * .5;
		let q2: number = q * .5;
		let n: number = 5, k: number;
		let tk: number, CFL: number, CFU: number, prob: number;
		//if(q<=0 || df<=0)
		//	throw new IllegalArgumentException("Illegal argument "+q+" or "+df+" for qnorm(p).");
		if (q < df) {
			tk = q2 * (1 - n - df2) / (df2 + 2 * n - 1 + n * q2 / (df2 + 2 * n));
			for (k = n - 1; k > 1; k--)
				tk = q2 * (1 - k - df2) / (df2 + 2 * k - 1 + k * q2 / (df2 + 2 * k + tk));
			CFL = 1 - q2 / (df2 + 1 + q2 / (df2 + 2 + tk));
			prob = Math.exp(df2 * Math.log(q2) - q2 - this.lnfgamma(df2 + 1) - Math.log(CFL));
		}
		else {
			tk = (n - df2) / (q2 + n);
			for (k = n - 1; k > 1; k--)
				tk = (k - df2) / (q2 + k / (1 + tk));
			CFU = 1 + (1 - df2) / (q2 + 1 / (1 + tk));
			prob = 1 - Math.exp((df2 - 1) * Math.log(q2) - q2 - this.lnfgamma(df2) - Math.log(CFU));
		}
		return prob;
	}



	/*  POCHISQ  --  probability of chi-square value

		  Adapted from:
				  Hill, I. D. and Pike, M. C.  Algorithm 299
				  Collected Algorithms for the CACM 1967 p. 243
		  Updated for rounding errors based on remark in
				  ACM TOMS June 1985, page 185
	 * 
	 * downloaded from: http://www.fourmilab.ch/rpkp/experiments/analysis/chiCalc.html
*/

	public static pochisq(x: number, df: number) {
		let a: number, s: number;
		let y: number = 0;
		let e: number, c: number, z: number;
		let even: boolean;                     /* True if df is an even number */
		let BIGX: number = 20.0;                  /* max value to represent exp(x) */

		const LOG_SQRT_PI: number = 0.5723649429247000870717135; /* log(sqrt(pi)) */
		const I_SQRT_PI: number = 0.5641895835477562869480795;   /* 1 / sqrt(pi) */

		if (x <= 0.0 || df < 1) {
			return 1.0;
		}

		a = 0.5 * x;
		even = (df % 2 == 0) ? true : false;
		if (df > 1) {
			y = this.ex(-a);
		}
		s = (even ? y : (2.0 * this.poz(-Math.sqrt(x))));
		if (df > 2) {
			x = 0.5 * (df - 1.0);
			z = (even ? 1.0 : 0.5);
			if (a > BIGX) {
				e = (even ? 0.0 : LOG_SQRT_PI);
				c = Math.log(a);
				while (z <= x) {
					e = Math.log(z) + e;
					s += this.ex(c * z - a - e);
					z += 1.0;
				}
				return s;
			} else {
				e = (even ? 1.0 : (I_SQRT_PI / Math.sqrt(a)));
				c = 0.0;
				while (z <= x) {
					e = e * (a / z);
					c = c + e;
					z += 1.0;
				}
				return c * y + s;
			}
		} else {
			return s;
		}
	}

	public static ex(x: number): number {
		let BIGX: number = 20.0;                  /* max value to represent exp(x) */

		return (x < -BIGX) ? 0.0 : Math.exp(x);
	}

	public static poz(z: number): number {
		let y: number, x: number, w: number;
		let Z_MAX: number = 6.0;              /* Maximum meaningful z value */

		if (z == 0.0) {
			x = 0.0;
		} else {
			y = 0.5 * Math.abs(z);
			if (y >= (Z_MAX * 0.5)) {
				x = 1.0;
			} else if (y < 1.0) {
				w = y * y;
				x = ((((((((0.000124818987 * w
					- 0.001075204047) * w + 0.005198775019) * w
					- 0.019198292004) * w + 0.059054035642) * w
					- 0.151968751364) * w + 0.319152932694) * w
					- 0.531923007300) * w + 0.797884560593) * y * 2.0;
			} else {
				y -= 2.0;
				x = (((((((((((((-0.000045255659 * y
					+ 0.000152529290) * y - 0.000019538132) * y
					- 0.000676904986) * y + 0.001390604284) * y
					- 0.000794620820) * y - 0.002034254874) * y
					+ 0.006549791214) * y - 0.010557625006) * y
					+ 0.011630447319) * y - 0.009279453341) * y
					+ 0.005353579108) * y - 0.002141268741) * y
					+ 0.000535310849) * y + 0.999936657524;
			}
		}
		return z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
	}

	public static lnfgamma(c: number) {
		let j: number;
		let x: number, y: number, tmp: number, ser: number;
		let cof: number[] = [76.18009172947146, -86.50532032941677,
			24.01409824083091, -1.231739572450155,
			0.1208650973866179e-2, -0.5395239384953e-5
		];
		y = x = c;
		tmp = x + 5.5 - (x + 0.5) * Math.log(x + 5.5);
		ser = 1.000000000190015;
		for (j = 0; j <= 5; j++)
			ser += (cof[j] / ++y);
		return (Math.log(2.5066282746310005 * ser / x) - tmp);
	}

	private constructor() { /* This works; without this line you can instanitate */ }

}