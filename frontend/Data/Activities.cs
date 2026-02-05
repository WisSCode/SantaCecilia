namespace frontend.Data;

public static class Activities
{
    public static readonly List<(string Id, string Name, string Category, decimal Rate)> ActivityList = new()
    {
        // Salario mínimo convencional
        ("min-wage", "Salario mínimo convencional", "general", 0.7011m),
        
        // Categoría General
        ("puente-conchero", "Hacer puente para conchero", "general", 0.7790m),
        ("herbicida", "Regar herbicida", "general", 0.7790m),
        ("nematodos", "Chequear nemátodos", "general", 0.7790m),
        ("desinfectar", "Desinfectar herramientas", "general", 0.7790m),
        ("fumigar", "Fumigar bolsas", "general", 0.7790m),
        ("empacadora", "Limpiar empacadora", "general", 0.7790m),
        ("pizote", "Botar pizote", "general", 0.7790m),
        ("banderero", "Banderero", "general", 0.7790m),
        
        // Mantenimiento
        ("nivelar", "Nivelar caminos", "maintenance", 0.8012m),
        ("ayudante-mecanico", "Ayudante mecánico", "maintenance", 0.8012m),
        ("ayudante-soldador", "Ayudante soldador", "maintenance", 0.8012m),
        ("celador", "Celador", "maintenance", 0.8123m),
        ("sanidad", "Sanidad (fin de vivienda/baños)", "maintenance", 0.8123m),
        ("irrigacion", "Ayudante irrigación", "maintenance", 0.8266m),
        
        // Técnicos especializados
        ("mecanico", "Mecánico", "specialized", 1.0126m),
        ("soldador", "Soldador", "specialized", 1.0126m),
        ("carpintero", "Carpintero", "specialized", 1.0126m),
        
        // Agricultura
        ("cedaceros", "Cedaceros", "agricultural", 0.7053m),
        ("reapuntalar", "Reapuntalar bananal", "agricultural", 0.7011m),
        ("leguminosas", "Sembrar leguminosas/gramíneas", "agricultural", 0.7715m),
        ("sigatoka", "Control de Sigatoka (deshoje)", "agricultural", 0.9368m),
        ("banderilla", "Desviar banderilla", "agricultural", 0.7164m),
        ("hospederas", "Sacar matas hospederas", "agricultural", 0.7935m),
        ("mantenimiento-canales", "Mantenimiento hierbas canales", "agricultural", 0.7715m),
        ("aporcar", "Aporcar semilleros", "agricultural", 0.7164m),
        ("tiburon", "Tiburón (herramienta)", "agricultural", 0.7584m),
        ("plantillo", "Mantenimiento de plantillo", "agricultural", 0.9042m),
        ("semillero", "Mantenimiento de semillero", "agricultural", 0.8955m),
        ("bambu", "Apuntalar con bambú", "agricultural", 0.7164m),
        ("poblacion", "Corregir población", "agricultural", 0.8955m),
        ("fruta", "Desviar fruta orilla caminos", "agricultural", 0.7011m),
        ("cargar-bambu", "Cargar bambú", "agricultural", 0.7011m),
        ("boquetes", "Limpieza de boquetes", "agricultural", 0.7011m),
    };
}
