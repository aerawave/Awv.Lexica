```
Have a 2% chance when struck in combat of increasing armor by $10342s1 for $10342d1.

Have a {ri(1,100)}(chance) chance when struck in combat of increasing armor by {ri(1,999)}(armor) for {rt(1s,60s)}


main () {
    return
        "Have a " +
        store("chance",
            ri(1, 100)
        ) +
        " chance when struck in combat of increasing armor by " +
        store("armor",
            ri(1, 999)
        ) +
        " for " +
        rt(1s, 60s)
    ;
}

store (key, value) {
    state[key] = value;
}

ri (min, max) => randomi(min, max);

randomi (min, max) {
    return floor(random() * (max - min)) + min;
}

rt (min, max) => randomt(min, max);

randomt (min, max) {
    // do stuff
}

round (val) {
    return floor(val + .5);
}

floor (val) {
    return val % 1;
}
```