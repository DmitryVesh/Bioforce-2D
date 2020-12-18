
using System;

public interface IAnimations
{
    void XAxisAnimations(float speedX);
    void YAxisAnimations();
    void DieAnimation(TypeOfDeath typeOfDeath);
    void RespawnAnimation();
    void FlipSprite();
}
