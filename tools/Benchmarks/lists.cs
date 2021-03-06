// $Id: lists.csharp,v 1.5 2005-02-22 19:05:07 igouy-guest Exp $
// http://shootout.alioth.debian.org/
// contributed by Erik Saltwell
// Some cleanups by Brent Fulgham
// Note:  C# standard libraries don't provide a DeQueue class, so this
//  implementation includes one.
using System;
using Common.Logging;

namespace Benchmarks.Lists
{
	public class IntDeQueue
	{
		private int[] data = null;
		private int start = 0;
		private int end = 0;
		private int size = 0;
		private int temp = 0;

		public bool Empty {
			get { return start == end; }
		}

		public object Clone ()
		{
			IntDeQueue temp = new IntDeQueue (size - 1);
			temp.start = start;
			temp.end = end;
			data.CopyTo (temp.data, 0);
			return temp;
		}

		public bool Equals (IntDeQueue other)
		{
			if (Count != other.Count)
				return false;

			int i = this.start;
			int iOther = other.start;

			while (i != this.end) {
				if (data [i] != other.data [iOther])
					return false;

				Advance (ref i);
				other.Advance (ref iOther);
			}
			return true;
		}

		public int Count {
			get {
				if (end >= start)
					return end - start;
				else
					return size + end - start; 
			}
		}

		public void Reverse ()
		{
			if (Count < 2)
				return;
			Array.Reverse (data);
			int endEnd = size - 1;
			int startEnd = 0;
			if (end < start) {
				endEnd = 0;
				startEnd = size - 1;
			}
			int temp = start;
			Regress (ref end);
			start = Math.Abs (startEnd - Math.Abs (end - endEnd));
			end = Math.Abs (endEnd - Math.Abs (temp - startEnd));
			Advance (ref end);
		}

		public void PushFront (int i)
		{
			temp = start;
			Regress (ref start);
			if (start == end) {
				start = temp;
				throw new System.Exception ("Invalid operation");
			}
			data [start] = i;
		}

		public int PopFront ()
		{
			int i = data [start];
			if (start != end)
				Advance (ref start);
			else
				throw new System.Exception ("Invalid operation");
			return i;
		}

		public int PeekFront ()
		{
			if (start == end)
				throw new System.Exception ("Invalid Operation");
			return data [start];
		}

		public int PeekBack ()
		{
			if (start == end)
				throw new System.Exception ("Invalid Operation");
			int temp = end;
			Regress (ref temp);
			return data [temp];
		}

		public void PushBack (int i)
		{
			temp = end;
			Advance (ref end);
			if (start == end) {
				end = temp;
				throw new System.Exception ("Invalid operation");
			}
			data [temp] = i;
		}

		public int PopBack ()
		{
			if (start != end)
				Regress (ref end);
			else
				throw new System.Exception ("Invalid operation");
			return data [end];
		}

		public IntDeQueue (int Size)
		{
			data = new int[Size + 1];
			this.size = Size + 1;
		}

		private void Advance (ref int item)
		{
			if ((++item) == size)
				item = 0;
		}

		private void Regress (ref int item)
		{
			if (item != 0)
				--item;
			else
				item = (size - 1);
		}

		public void Clear ()
		{
			start = 0;
			end = 0;
		}
	}

	public class Lists
	{
		public const int SIZE = 10000;
		static ILog logger;

		public static void Main (string[] args, ILog ilog)
		{
			logger = ilog;
			int n = int.Parse (args [0]);
			int result = 0;
			for (int i = 0; i < n; ++i)
				result = RunLists ();
			logger.InfoFormat (result + "");
		}

		static public int RunLists ()
		{
			IntDeQueue q = new IntDeQueue (SIZE);
			for (int i = 0; i < SIZE; ++i)
				q.PushBack (i + 1);
			IntDeQueue q2 = (IntDeQueue)q.Clone ();
			IntDeQueue q3 = new IntDeQueue (SIZE);
			while (!q2.Empty)
				q3.PushBack (q2.PopFront ());
			while (!q3.Empty)
				q2.PushBack (q3.PopBack ());
			q.Reverse ();
			if (q.PeekFront () != SIZE) {
				logger.InfoFormat ("q.PeekFront()!=SIZE");
				return 0;
			}
			if (!q.Equals (q2)) {
				logger.InfoFormat ("q!=q2");
				return 0;
			}

			return q.Count;
		}
	}
}
