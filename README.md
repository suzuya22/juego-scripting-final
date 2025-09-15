# juego-scripting-final
Juego para scripting (codigo fuente y avances)

## equipo
-Juan Sebastian Lopez Martinez 000492240  
-Juan Nicolas Correa Lopez

## Correciones
Desde la primera propuesta de diseño direccionamos el proyecto del juego teniendo en cuenta las correciones dadas por el docente

1. el juego cuenta con una mecanica de quick time, donde debes avanzar rapido por el nivel presionando las teclas adecuadas para romper los bloques  
2. La penalizacion de cada nivel es al incumplir con el contador para avanzar el nivel, esto provoca que el nivel se reinicie (el personaje vuelve al inicio(manejado por una id del tile en la malla))
3. es un juego 2D que cuenta con una tematica pixel art, con diseño tilemap (muy importante), cuenta con una camara donde se ve el escenario
4. tenemos un planteamiento de 3 niveles, la principal razon por la que diseñamos el flujo del juego de esta forma es porque maximiza la facilidad para crear niveles, es similar a juegos como paper mario, o el editor de geometry dash, donde solo asignas valores de la malla a los sprite y por el flujo estos se convierten en un bloque con funciondalidad especifica, siendo la parte mas comoda del desarrollo
5. El temporizador funciona como la forma de castigo en el nivel, es una cuenta hacia atras vista en pantalla en donde el jugador debe completar el nivel antes de que este termine (el retroceso representa volver a la casilla inicial del spawn)
6. las herramientas usadas fueron las vistas en clase, implementamos el arbol de estructura de datos jerarquica y las relaciones entre clases de programacion orientada a objetos.
7. La generacion procedural fue un planteamiento anterior, aun pensamos si poner un modo "infinito" donde se use esa generacion procedural y no solo tener los niveles diseñados por nosotros, por la matriz es algo mas manejable, pero nos gustaria consultar eso con el docente.


Decidimos continuar con la propuesta inicial apesar de las correciones debido a que nos parece una buena forma diseñar un videojuego de este estilo, pensamos mucho en como el sistema aporta facilidad y eficacia para diseñar niveles, tomamos muy en cuenta los comentarios dados por el docente en la revision de la propuesta y es por eso que decidimos ampliar de mejor manera la explicacion del juego

## checklist de cosas que debemos implementar (se tachan aqui mediante los commits futuros)
- correciones de bugs visuales debido a fallos con los pivots del sprite y los tile, generando que se vea como si estuviesen sobre el otro  
- implementacion de mecanicas que mejoren el gameplay sin dejarlo monotono
modo infinito  
- destruir bloques da tiempo al contador y hacer el tiempo muy justo para que sea mas notoria la necesidad de ser preciso y rapido al jugar  
- testeo de bloques de mas de un key necesario para ser destruido (la logica esta pero faltan pruebas unitarias al respecto)
- diseño estetica del juego (temas de diseño)
- implementacion del resto de niveles y UI

   
## elementos externos usados
https://incolgames.itch.io/dungeon-platformer-tile-set-pixel-art - tileset de libre uso usado para las pruebas (esta definido que crearemos el propio, junto a los sprites de herramientas, personaje y UI)

## ejecutable
el ejecutable esta dentro de la carpeta del juego, el mas reciente sera siempre el que tenga el .rar, ese se debe descargar
   
