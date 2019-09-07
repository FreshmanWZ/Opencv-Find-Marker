using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchTemplate
{
  public  class MyStructrue
    {
    }
    public class Node<T> {
        public T data;
        public Node<T> next;
        public bool first;
        public bool end;
        public Node() {

        }
    }
    public class LinkedList<T> {
        Node<T>[] nodes;
        int size;
        
    }

    /// <summary>
    /// 循环链表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircleSeuence<T> {
        int size;
        Node<T>[] nodes;
        public CircleSeuence(int size) {
            if (size<0) {
                return;
            }
            this.size = size;
            nodes = new Node<T>[this.size];
            for (int i=0;i<size;i++) {
                nodes[i] = new Node<T>();
            }
            
            for (int i=0;i<size;i++) {
            
                if (i==0) {
                    nodes[0].first = true;
                }
                if (i==size-1) {
                    nodes[i].next = nodes[0];
                    nodes[i].end = true;
                    break;
                }
                nodes[i].next = nodes[i + 1];
            }
        }
        public Node<T> getFirst() {
            if(size>0)
            return nodes[0];
            return null;
        }
        public int getSize() {
            return size;
        }
        /// <summary>
        /// 给对应的节点赋值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        public void setValue(int index,T data) {
            if (index>0&&index<size) {
                nodes[index].data = data;
            }
        }
        public void insertAfter(int left,T value) {
            
            if (left > size - 1)
            {
                throw new IndexOutOfRangeException();
            }
            
            else {
                Node<T> rightNode = nodes[left].next;
                Node<T> insertNode = new Node<T>();
                insertNode.data = value;
                nodes[left].next = insertNode;
                insertNode.next = rightNode;
            }
            size++;
        }
        
    }
    public class CircleIterator<T> {
        int currentIndex = -1;
        Node<T> currentNode;
        CircleSeuence<T> circleSeuence;
        public CircleIterator(CircleSeuence<T> circleSeuence) {
            this.circleSeuence = circleSeuence;
            currentNode = circleSeuence.getFirst();
        }
        public T next() {
            if (currentIndex==-1) {
                currentNode = circleSeuence.getFirst();
                currentIndex++;
                return currentNode.data;
            }
            currentIndex++;
            currentNode = currentNode.next;
            return currentNode.data;
             
        }

    }
}
