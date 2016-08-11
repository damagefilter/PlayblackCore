using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playblack.BehaviourTree.Model.Core {

    public class Position {

        /// <summary>
        /// The list of moves that this position represents
        /// </summary>
        private ICollection<int> moves;

        public ICollection<int> Moves {
            get {
                // TODO: Check if this is modified outside, if not don't return a copy
                return moves.ToList();
            }
        }

        public Position(params int[] moves) {
            this.moves = new LinkedList<int>();
            // TODO: perhaps there is some addall functionality somewhrere?
            foreach (int i in moves) {
                this.moves.Add(i);
            }
        }

        public Position(ICollection<int> moves) {
            this.moves = new LinkedList<int>(moves);
        }

        public Position(Position copy) {
            this.moves = new LinkedList<int>(copy.moves);
        }

        public Position AddMove(int move) {
            this.moves.Add(move);
            return this;
        }

        public Position AddMoves(ICollection<int> moves) {
            foreach (int i in moves) {
                this.moves.Add(i);
            }
            return this;
        }

        public override string ToString() {
            StringBuilder result = new StringBuilder();
            if (this.moves.Count > 0) {
                foreach (int i in this.moves) {
                    result.Append(i).Append(" ");
                }
                result.Remove(result.Length - 1, 1);
                return "[" + result.ToString() + "]";
            }
            else {
                return "[]";
            }
        }

        public override bool Equals(object o) {
            if (this == o) {
                return true;
            }

            if (!(o is Position)) {
                return false;
            }

            Position oPosition = (Position)o;

            ICollection<int> thisMoves = this.Moves;
            ICollection<int> oMoves = oPosition.Moves;

            if (oMoves.Count != thisMoves.Count) {
                return false;
            }

            IEnumerator<int> thisIt = thisMoves.GetEnumerator();
            IEnumerator<int> oIt = oMoves.GetEnumerator();

            while (thisIt.MoveNext()) {
                int thisElem = thisIt.Current;
                oIt.MoveNext();
                int oElem = oIt.Current;

                if (thisElem != oElem) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            return this.moves.GetHashCode();
        }
    }
}