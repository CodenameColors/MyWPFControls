﻿using System;
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

namespace TimelinePlayer.Components
{
	public class Timeline : Canvas
	{

		public LinkedList<TimeBlock> timeBlocksLL
		{
			get { return LLTimeBlocks; }
			set { LLTimeBlocks = new LinkedList<TimeBlock>(value); }
		}
		private LinkedList<TimeBlock> LLTimeBlocks = new LinkedList<TimeBlock>();

		public Timeline()
		{
			timeBlocksLL = new LinkedList<TimeBlock>();
		}

		/// <summary>
		/// this method is here to INSERT the time block into the timeline's linked list in the CORRECT spot
		/// </summary>
		public void AddTimeBlock(TimeBlock timeBlock)
		{
			LLTimeBlocks.AddLast(timeBlock);
			this.Children.Add(timeBlock);
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
