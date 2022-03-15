using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/GradientExtended")]
public class UIGradient_Extended : BaseMeshEffect
{
    public Gradient m_colors;
    [Range(-180f, 180f)]
    public float m_angle = 0f;
    public bool m_ignoreRatio = true;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (enabled)
        {
            Rect rect = graphic.rectTransform.rect;
            Vector2 dir = UIGradientUtils.RotationDir(m_angle);

            if (!m_ignoreRatio)
                dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

            UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            UIVertex vertex = default(UIVertex);
            for(int colIndex = 0; colIndex < m_colors.colorKeys.Length-1; colIndex++)
            {
                int start = vh.currentVertCount * (int)m_colors.colorKeys[colIndex].time;
                int end = vh.currentVertCount * (int)m_colors.colorKeys[colIndex+1].time;
                for (int i = start; i < end; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 localPosition = localPositionMatrix * vertex.position;
                    vertex.color *= Color.Lerp(m_colors.colorKeys[colIndex].color, m_colors.colorKeys[colIndex+1].color, localPosition.y);
                    vh.SetUIVertex(vertex, i);
                }
            }

        }
    }
}
