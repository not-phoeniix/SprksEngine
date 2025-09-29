using System.Diagnostics;
using Sprks.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Scenes;

internal class Octree<T> where T : class, ITransform3D {
    // https://medium.com/@bpmw/quadtrees-for-2d-games-with-moving-elements-63360b08329f

    private static readonly int splitThreshold = 50;

    private class Node {
        private readonly List<T> data;
        private readonly List<(T, Node)> toMove;
        private Node[]? childNodes;
        private readonly Node? parent;

        public BoundingBox Bounds { get; private set; }

        public Node(Node? parent, BoundingBox bounds) {
            data = new List<T>();
            toMove = new List<(T, Node)>();
            this.parent = parent;
            this.Bounds = bounds;
        }

        public void Clear() {
            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    node.Clear();
                }
            }

            data.Clear();
            childNodes = null;
        }

        public void Insert(T obj) {
            if (obj.Transform == null) Debug.WriteLine("AHHHH!!!!");

            // if we can't fit into this node, try inserting into the parent!
            if (!InNode(obj, this) && parent != null) {
                parent.Insert(obj);
                return;
            }

            if (childNodes == null) {
                // if children nodes don't exist yet...
                if (data.Count < splitThreshold) {
                    // and we're below threshold,,, just add!
                    data.Add(obj);
                } else {
                    // otherwise, split and reorganize...
                    Split();
                    data.Add(obj);
                    // Reorganize();
                }

            } else {
                // if we have children...
                foreach (Node node in childNodes) {
                    // search each node, try to add to children here
                    if (InNode(obj, node)) {
                        node.Insert(obj);
                        return;
                    }
                }

                // if can't be added to children just add here lol
                data.Add(obj);
            }
        }

        private void Split() {
            Node MakeChild(int xOffset, int yOffset, int zOffset) {
                Vector3 halfSize = (Bounds.Max - Bounds.Min) / 2.0f;

                return new Node(
                    this,
                    new BoundingBox(
                        Bounds.Min + new Vector3(
                            halfSize.X * xOffset,
                            halfSize.Y * yOffset,
                            halfSize.Z * zOffset
                        ),
                        Bounds.Max + new Vector3(
                            halfSize.X * (xOffset + 1),
                            halfSize.Y * (yOffset + 1),
                            halfSize.Z * (zOffset + 1)
                        )
                    )
                );
            }

            childNodes = new Node[8] {
                MakeChild(0, 0, 0),
                MakeChild(1, 0, 0),
                MakeChild(0, 0, 1),
                MakeChild(1, 0, 1),
                MakeChild(0, 1, 0),
                MakeChild(1, 1, 0),
                MakeChild(0, 1, 1),
                MakeChild(1, 1, 1),
            };
        }

        public void Reorganize() {
            // iterate across data and mark data that should be moved as "to move"
            foreach (T obj in data) {
                // if this actor no longer fits in this node,
                //   remove it from this one and try to add to parent!
                if (!InNode(obj, this) && parent != null) {
                    // data.RemoveAt(i);
                    // parent.Insert(obj);
                    toMove.Add((obj, parent));
                    continue;
                }

                // if any child nodes fit the actor perfectly,
                //   add it to that one instead lol
                if (childNodes != null) {
                    foreach (Node node in childNodes) {
                        if (InNode(obj, node)) {
                            // data.RemoveAt(i);
                            // node.Insert(obj);
                            toMove.Add((obj, node));
                            break;
                        }
                    }
                }
            }

            // move all data that was marked as "to move"
            foreach ((T, Node) objPair in toMove) {
                data.Remove(objPair.Item1);
                objPair.Item2.Insert(objPair.Item1);
            }
            toMove.Clear();
        }

        private static bool InNode(T obj, Node node) {
            return node.Bounds.Contains(obj.Transform.GlobalPosition) == ContainmentType.Contains;
        }

        public bool Remove(T obj) {
            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    if (InNode(obj, node)) {
                        return node.Remove(obj);
                    }
                }
            }

            return data.Remove(obj);
        }

        public T? FindClosest(Vector3 position) {
            T? closest = null;
            float closestDSqr = float.PositiveInfinity;

            foreach (T obj in data) {
                float dSqr;
                if (obj is IActor3D actor) {
                    dSqr = Utils.DistanceSquared(position, actor.Bounds);
                } else {
                    dSqr = Vector3.DistanceSquared(position, obj.Transform.GlobalPosition);
                }

                if (dSqr < closestDSqr) {
                    closest = obj;
                    closestDSqr = dSqr;
                }
            }

            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    // don't search nodes that are further away than
                    //   already found closest node
                    float dSqr = Utils.DistanceSquared(position, node.Bounds);
                    if (dSqr >= closestDSqr) continue;

                    // recursive call here!!!
                    T? subClosest = node.FindClosest(position);

                    if (subClosest != null) {
                        dSqr = Vector3.DistanceSquared(subClosest.Transform.GlobalPosition, position);
                        if (dSqr < closestDSqr) {
                            closest = subClosest;
                            closestDSqr = dSqr;
                        }
                    }
                }
            }

            return closest;
        }

        public IEnumerable<T> GetData(Vector3 position, float radius, bool reorganize) {
            float dSqrToThis = Utils.DistanceSquared(position, Bounds);
            if (dSqrToThis > radius * radius) yield break;

            foreach (T obj in data) {
                float dSqr;
                if (obj is IActor3D actor) {
                    dSqr = Utils.DistanceSquared(position, actor.Bounds);
                } else {
                    dSqr = Vector3.DistanceSquared(position, obj.Transform.GlobalPosition);
                }

                if (dSqr <= radius * radius) {
                    yield return obj;
                }
            }

            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    foreach (T obj in node.GetData(position, radius, reorganize)) {
                        yield return obj;
                    }
                }
            }

            // reorganize after allllllll returning has finished
            if (reorganize) {
                Reorganize();
            }
        }

        public IEnumerable<T> GetData(BoundingFrustum viewport, bool reorganize) {
            if (!viewport.Intersects(Bounds)) yield break;

            foreach (T obj in data) {
                bool contains;
                if (obj is IActor3D actor) {
                    ContainmentType containment = viewport.Contains(actor.Bounds);
                    contains = containment == ContainmentType.Contains ||
                               containment == ContainmentType.Intersects;
                } else {
                    contains = viewport.Contains(obj.Transform.GlobalPosition) == ContainmentType.Contains;
                }

                if (contains) {
                    yield return obj;
                }
            }

            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    foreach (T obj in node.GetData(viewport, reorganize)) {
                        yield return obj;
                    }
                }
            }

            // reorganize after allllllll returning has finished
            if (reorganize) {
                Reorganize();
            }
        }

        public IEnumerable<T> GetData(bool reorganize) {
            foreach (T obj in data) {
                yield return obj;
            }

            if (childNodes != null) {
                foreach (Node node in childNodes) {
                    foreach (T obj in node.GetData(reorganize)) {
                        yield return obj;
                    }
                }
            }

            // reorganize after allllllll returning has finished
            if (reorganize) {
                Reorganize();
            }
        }
    }

    private readonly Node root;

    public Octree(Vector3 min, Vector3 max) {
        root = new Node(null, new BoundingBox(min, max - min));
    }

    public void Insert(T obj) {
        root.Insert(obj);
    }

    public T? FindClosest(Vector3 position) {
        return root.FindClosest(position);
    }

    public IEnumerable<T> GetData(Vector3 position, float radius, bool reorganize) {
        return root.GetData(position, radius, reorganize);
    }

    public IEnumerable<T> GetData(BoundingFrustum viewport, bool reorganize) {
        return root.GetData(viewport, reorganize);
    }

    public IEnumerable<T> GetData(bool reorganize) {
        return root.GetData(reorganize);
    }

    public bool Remove(T obj) {
        return root.Remove(obj);
    }

    public void Clear() {
        root.Clear();
    }
}

