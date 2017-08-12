# MotionStripes
PostEffect that adds motion stripes to every moving object - /!\ work in progress /!\

Inspired by motions in anime:

![dbz](https://user-images.githubusercontent.com/2204781/29171672-98bb8212-7ddd-11e7-8e60-399606577a33.gif)


solid black trails

![motiontrail](https://user-images.githubusercontent.com/2204781/29149435-73e27c48-7d74-11e7-962e-e32f642c9074.gif)

trails affected by a lookup table which is used in combination with the amplitude of the movement

![motiontrail2](https://user-images.githubusercontent.com/2204781/29171775-f2d67d4c-7ddd-11e7-815a-a30d01358a68.gif)

you can blend between an arbitrary which will be set by the lookup table and the color of the object itself

![motiontrail2](https://user-images.githubusercontent.com/2204781/29244667-09b98748-7fbe-11e7-8b75-217680211ed5.gif)

next step: adding blurred lines to do as if some of them were kind of stuck in place: 

![anime](https://user-images.githubusercontent.com/2204781/29149533-0973c6b8-7d75-11e7-89c5-f9ade88b9631.jpg)

Thanks to Keijiro Takahashi (https://github.com/keijiro/) and his work on motion vectors (the project began as a fork of Kino/Vision, which is a post-process that shows motion vectors on the screen https://github.com/keijiro/KinoVision/)
