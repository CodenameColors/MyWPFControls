using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimelinePlayer.Components
{
	public class Timeline : Canvas
	{
		public String TrackName = "";
		public String TrackImagePath = null;
		public double TimePerPixel;
		public LinkedListNode<TimeBlock> ActiveBlock;
		public LinkedList<TimeBlock> timeBlocksLL
		{
			get { return LLTimeBlocks; }
			set { LLTimeBlocks = new LinkedList<TimeBlock>(value); }
		}


		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(Timeline), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

		private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var control = sender as Timeline;
			if (control != null)
				control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
		}

		private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			// Remove handler for oldValue.CollectionChanged
			if (oldValue != null)
			{
				((ObservableCollection<Timeline>)oldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
			// Add handler for newValue.CollectionChanged (if possible)
			if (null != newValue)
			{
				((ObservableCollection<Timeline>)newValue).CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
			}
		}

		void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//Do your stuff here.

		}

		public bool TimelineisNull_flag { get; set; }
		private LinkedList<TimeBlock> LLTimeBlocks = new LinkedList<TimeBlock>();

		public Timeline(int TimeWidth, double InitialSize)
		{
			this.HorizontalAlignment = HorizontalAlignment.Left;
			Margin = new Thickness(0);
			timeBlocksLL = new LinkedList<TimeBlock>();
			TimePerPixel = 1.0 / TimeWidth;
			ActiveBlock = timeBlocksLL.First;
			this.SizeChanged += Timeline_SizeChanged;
			this.Width = InitialSize;
		}

		private void Timeline_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			foreach (TimeBlock tblock in LLTimeBlocks)
			{
				tblock.ScaleToTimeline();
				//tblock.RenderTransform = this.RenderTransform;
			}
		}

		public void SetRenderTransform(double initialsize, double Scalefactor, double TimeWidthPix)
		{
			
			this.TimePerPixel = 1.0 / TimeWidthPix;
			foreach (TimeBlock tblock in LLTimeBlocks)
			{
				//tblock.RenderTransform = this.RenderTransform;
			}
		}

		/// <summary>
		/// this method is here to INSERT the time block into the timeline's linked list in the CORRECT spot
		/// </summary>
		public void AddTimeBlock(TimeBlock timeBlock, double timeWidth, bool resetptr = true)
		{
			
			LLTimeBlocks.AddLast(timeBlock);
			Canvas.SetLeft(timeBlock, timeBlock.StartTime *  timeWidth);
			if (timeBlock.Duration > 0.0)
				timeBlock.Width = timeWidth * (timeBlock.EndTime - timeBlock.StartTime);
			else
				timeBlock.Width = timeWidth * 2;
			
			{
				var parentObject = VisualTreeHelper.GetParent(timeBlock);
				if (parentObject != null)
				{
					(parentObject as Canvas).Children.Remove(timeBlock);
				}
				this.Children.Add(timeBlock);
			}
			if(resetptr)
				ActiveBlock = timeBlocksLL.First;
		}



		public void SetActiveBlock(double CurTime)
		{
			if(ActiveBlock.Next != null && CurTime > ActiveBlock.Value.EndTime)
			{
				ActiveBlock = ActiveBlock.Next; //advance
			}
			else if(ActiveBlock.Next == null && CurTime > ActiveBlock.Value.EndTime)
			{
				ActiveBlock = null;
			}
		}

		public void InitActiveBlock(double CurTime)
		{
			SortTimeBlocksLL();
			LinkedListNode<TimeBlock> curBlock = LLTimeBlocks.First;
			while(curBlock != null)
			{
				if (CurTime >= curBlock.Value.StartTime && CurTime <= curBlock.Value.EndTime)
				{
					ActiveBlock = curBlock;
					return;
				}
				curBlock = curBlock.Next;
			}
			ActiveBlock = LLTimeBlocks.First;
		}

		public void SortTimeBlocksLL()
		{
			List<TimeBlock> TempTBlocks = LLTimeBlocks.ToList();
			TempTBlocks = (List<TimeBlock>)(TempTBlocks.OrderBy(o => o.StartTime).ToList());
			LLTimeBlocks.Clear();
			foreach (TimeBlock tblock in TempTBlocks)
			{
				LLTimeBlocks.AddLast(tblock);
			}
		}

		public void ClearLLTimeBlocks()
		{
			LLTimeBlocks.Clear();
		}

		public bool SetActiveTBlock(TimeBlock desiredTimeBlock)
		{
			bool retb = false;
			LinkedListNode<TimeBlock> TBN = LLTimeBlocks.First;
			while (TBN != null)
			{
				if (TBN.Value == desiredTimeBlock)
				{
					ActiveBlock = TBN;
					retb = true;
					break;
				}
			}

			return retb;
		}

	}
	/// <summary>
	/// This class is a LinkedList that can be used in a WPF MVVM scenario. Composition was used instead of inheritance,
	/// because inheriting from LinkedList does not allow overriding its methods.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ObservableLinkedList<T> : INotifyCollectionChanged, IEnumerable
	{
		private LinkedList<T> m_UnderLyingLinkedList;

		#region Variables accessors
		public int Count
		{
			get { return m_UnderLyingLinkedList.Count; }
		}

		public LinkedListNode<T> First
		{
			get { return m_UnderLyingLinkedList.First; }
		}

		public LinkedListNode<T> Last
		{
			get { return m_UnderLyingLinkedList.Last; }
		}
		#endregion

		#region Constructors
		public ObservableLinkedList()
		{
			m_UnderLyingLinkedList = new LinkedList<T>();
		}

		public ObservableLinkedList(IEnumerable<T> collection)
		{
			m_UnderLyingLinkedList = new LinkedList<T>(collection);
		}
		#endregion

		#region LinkedList<T> Composition
		public LinkedListNode<T> AddAfter(LinkedListNode<T> prevNode, T value)
		{
			LinkedListNode<T> ret = m_UnderLyingLinkedList.AddAfter(prevNode, value);
			OnNotifyCollectionChanged();
			return ret;
		}

		public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			m_UnderLyingLinkedList.AddAfter(node, newNode);
			OnNotifyCollectionChanged();
		}

		public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
		{
			LinkedListNode<T> ret = m_UnderLyingLinkedList.AddBefore(node, value);
			OnNotifyCollectionChanged();
			return ret;
		}

		public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
		{
			m_UnderLyingLinkedList.AddBefore(node, newNode);
			OnNotifyCollectionChanged();
		}

		public LinkedListNode<T> AddFirst(T value)
		{
			LinkedListNode<T> ret = m_UnderLyingLinkedList.AddFirst(value);
			OnNotifyCollectionChanged();
			return ret;
		}

		public void AddFirst(LinkedListNode<T> node)
		{
			m_UnderLyingLinkedList.AddFirst(node);
			OnNotifyCollectionChanged();
		}

		public LinkedListNode<T> AddLast(T value)
		{
			LinkedListNode<T> ret = m_UnderLyingLinkedList.AddLast(value);
			OnNotifyCollectionChanged();
			return ret;
		}

		public void AddLast(LinkedListNode<T> node)
		{
			m_UnderLyingLinkedList.AddLast(node);
			OnNotifyCollectionChanged();
		}

		public void Clear()
		{
			m_UnderLyingLinkedList.Clear();
			OnNotifyCollectionChanged();
		}

		public bool Contains(T value)
		{
			return m_UnderLyingLinkedList.Contains(value);
		}

		public void CopyTo(T[] array, int index)
		{
			m_UnderLyingLinkedList.CopyTo(array, index);
		}

		public bool LinkedListEquals(object obj)
		{
			return m_UnderLyingLinkedList.Equals(obj);
		}

		public LinkedListNode<T> Find(T value)
		{
			return m_UnderLyingLinkedList.Find(value);
		}

		public LinkedListNode<T> FindLast(T value)
		{
			return m_UnderLyingLinkedList.FindLast(value);
		}

		public Type GetLinkedListType()
		{
			return m_UnderLyingLinkedList.GetType();
		}

		public bool Remove(T value)
		{
			bool ret = m_UnderLyingLinkedList.Remove(value);
			OnNotifyCollectionChanged();
			return ret;
		}

		public void Remove(LinkedListNode<T> node)
		{
			m_UnderLyingLinkedList.Remove(node);
			OnNotifyCollectionChanged();
		}

		public void RemoveFirst()
		{
			m_UnderLyingLinkedList.RemoveFirst();
			OnNotifyCollectionChanged();
		}

		public void RemoveLast()
		{
			m_UnderLyingLinkedList.RemoveLast();
			OnNotifyCollectionChanged();
		}
		#endregion

		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public void OnNotifyCollectionChanged()
		{
			if (CollectionChanged != null)
			{
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (m_UnderLyingLinkedList as IEnumerable).GetEnumerator();
		}

		#endregion
	}


}
