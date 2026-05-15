using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SkewedBar : Graphic
{
    // Horizontal skew — shifts top-left / top-right corners on X (visible on vertical bars)
    [SerializeField] float _topLeftSkew;
    [SerializeField] float _topRightSkew;

    // Vertical tilt — shifts entire left / right side up or down
    [SerializeField] float _leftYOffset;
    [SerializeField] float _rightYOffset;

    // Edge inset — makes one side shorter by pulling top corner down and bottom corner up
    // e.g. rightEdgeInset = 60 → right side is 120px shorter than left (trapezoid)
    [SerializeField] float _leftEdgeInset;
    [SerializeField] float _rightEdgeInset;

    public float topLeftSkew    { get => _topLeftSkew;    set { _topLeftSkew    = value; SetVerticesDirty(); } }
    public float topRightSkew   { get => _topRightSkew;   set { _topRightSkew   = value; SetVerticesDirty(); } }
    public float leftYOffset    { get => _leftYOffset;    set { _leftYOffset    = value; SetVerticesDirty(); } }
    public float rightYOffset   { get => _rightYOffset;   set { _rightYOffset   = value; SetVerticesDirty(); } }
    public float leftEdgeInset  { get => _leftEdgeInset;  set { _leftEdgeInset  = value; SetVerticesDirty(); } }
    public float rightEdgeInset { get => _rightEdgeInset; set { _rightEdgeInset = value; SetVerticesDirty(); } }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();

        var bl = new Vector2(r.xMin,                 r.yMin + _leftYOffset  + _leftEdgeInset);
        var br = new Vector2(r.xMax,                 r.yMin + _rightYOffset + _rightEdgeInset);
        var tl = new Vector2(r.xMin + _topLeftSkew,  r.yMax + _leftYOffset  - _leftEdgeInset);
        var tr = new Vector2(r.xMax + _topRightSkew, r.yMax + _rightYOffset - _rightEdgeInset);

        vh.AddVert(bl, color, Vector2.zero);
        vh.AddVert(br, color, Vector2.right);
        vh.AddVert(tr, color, Vector2.one);
        vh.AddVert(tl, color, Vector2.up);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }
}
