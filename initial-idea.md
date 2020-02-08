```
Have a 2% chance when struck in combat of increasing armor by $10342s1 for $10342d1.

Have a {ri(1,100)}(chance) chance when struck in combat of increasing armor by {ri(1,999)}(armor) for {rt(1s,60s)}


main () {
    return
        "Have a " +
        store("chance",
            ri
        )
}

store (key, value) {
    state[key] = value;
}

ri (min, max) => randomi(min, max);

randomi (min, max) {
    return floor(random() * (max - min)) + min;
}

round (val) {
    return floor(val + .5);
}

floor (val) {
    return val % 1;
}
```