using UnityEngine;

namespace MergeGame {
    // Shape types
    public enum ItemShape {
        Circle,
        Square,
        Cube
    }

    // Effects after merge
    public enum ItemEffect {
        None,
        Explosion,
        Upgrade,
        ScoreBoost
    }

    // Sound types
    public enum SoundType {
        Drop,
        Merge,
        Destroy
    }
}