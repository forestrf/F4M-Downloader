using System;
using System.Collections.Generic;
using System.IO;

public static class HTML
{

	public static string getCabeceraHTML(){
		return "<html>" +
			"<head>" +
			"<title>F4M-Downloader V" + MainClass.version + "</title>" +
			"<link rel=\"stylesheet\" href=\"all.css\">" +
			"<script src=\"http://code.jquery.com/jquery-2.0.3.min.js\"></script>" +
				"<script type=\"text/javascript\">" +

				"var _gaq = _gaq || [];" +
				"_gaq.push(['_setAccount', 'UA-29252510-2']);" +
				"_gaq.push(['_trackPageview']);" +

				"(function() {" +
				"var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;" +
				"ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';" +
				"var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);" +
				"})();" +

				"</script>" +
			"</head>" +
			"<body>" +
			"<div id=\"cerrarAplicacion\">" +
			"<a href=\"/?accion=cerrarPrograma\" onclick=\"return confirm('Se cancelarán todas las descargas en progreso\\n¿Seguro que quieres cerrar el programa?');\">Cerrar F4M-Downloader</a>" +
			"</div>" +
			"<div id=\"menu\">" +
			"<a href=\"/\" class=\"titulo_menu\">F4M-Downloader V" + MainClass.version + "</a>" +
			"<a href=\"http://www.descargavideos.tv\">Descargavideos.TV</a>" +
			"<a href=\"http://www.descargavideos.tv/lab#lab_f4m-downloader\" target=\"_blank\">Buscar actualizaciones</a>" +
			"<a href=\"/opciones\">Opciones</a>" +
			"<a href=\"/ayuda\">Ayuda</a>" +
			"</div>";
		
	}

	public static string getOpciones(){
		return getCabeceraHTML () +
			"<div id=\"contenido\">" +
			"<div class='contenidoTIT'>Dónde se guardarán la descargas:</div>" +
			"<div class='contenidoCONT'>Ruta actual: "+MainClass.configs.rutaDescargas+"<br>" +
			"<a href='/listadirs?ruta="+MainClass.configs.rutaDescargas+"'>Elejir dónde guardar los vídeos</a></div>" +
			"<div class='contenidoTIT'>Configurar un proxy:</div>" +
			"<form method='GET' action='/elijeproxy'>"+
			"<div class='contenidoCONT'>Proxy: <input type='text' name='valor' placeholder='x.x.x.x:xxxx' value='"+MainClass.configs.proxy+"'>" +
			"<input type='submit' value='Usar este proxy'><br>" +
			"Para no usar proxy guarda el campo Proxy vacio." +
			"</form>"+
			"</div>";
	}

	public static string getIndex(){
		return getCabeceraHTML() +
				"<div id=\"contenido\">" +
					"Nueva descarga:" +
					"<form id=\"form_agregar\" class=\"tabla\" method=\"GET\">" +
						"<input type=\"hidden\" name=\"accion\" value=\"descargar\">" +
						"<input type=\"hidden\" name=\"cerrarVentana\" value=\"0\">" +
						"<div class=\"elemento\">" +
							"<div>Comando: </div><div><input type=\"text\" name=\"url\" size=\"60\" placeholder=\"--manifest url f4m\"></div>" +
						"</div>" +
						"<div class=\"elemento\">" +
							"<div>Nombre: </div><div><input type=\"text\" name=\"nombre\" size=\"60\" value=\"\" placeholder=\"nombre del archivo de vídeo\"></div>" +
						"</div>" +
						"<div class=\"elemento\">" +
							"<div><input type=\"submit\" value=\"Agregar\"></div><div></div>" +
						"</div>" +
					"</form>" +
					"Descargas en progreso:" +
					"<div id=\"descargando\" class=\"tabla\">" +
						getProgreso() +
					"</div>" +
				"</div>" +
				"<script>" +
					"setInterval(actualizaDescargando, 1000);" +
					"function actualizaDescargando(){" +
						"$.ajax({" +
							"url: \"/?accion=progreso\"" +
						"})" +
						".done(function( res ) {" +
							"$( \"#descargando\" ).html( res );" +
						"});" +
					"}" +
				"</script>" +
			"</body>" +
			"</html>";
	}

	public static string getProgreso(){
		return getProgreso("");
	}
	
	public static string getProgreso(string mensaje){
		String resp = "<div class=\"elemento titulos\">" +
							"<div class=\"n\">Nombre</div>" +
		              "<div class=\"u\">Comando</div>" +
							"<div class=\"p\">Progreso</div>" +
							"<div class=\"t\">Tiempo restante</div>" +
							"<div class=\"q\">Quitar</div>" +
						"</div>";
		for (int i=0; i<MainClass.descargasEnProceso.Count; i++) {
			if (MainClass.descargasEnProceso [i].fallado != "") {
				resp += "<div class=\"elemento\">" +
					"<div class=\"n\">" + MainClass.descargasEnProceso [i].nombre + "</div>" +
					"<div class=\"u\">" + MainClass.descargasEnProceso [i].url + "</div>" +
					"<div class=\"p\">"+MainClass.descargasEnProceso [i].fallado+"</div>" +
					"<div class=\"t\"></div>" +
					"<div class=\"q\"><a href=\"/?accion=cancelarDescarga&elem=" + i + "\">Quitar</a></div>" +
					"</div>";
			} else {
				resp += "<div class=\"elemento\">" +
					"<div class=\"n\">" + MainClass.descargasEnProceso [i].nombre + "</div>" +
					"<div class=\"u\">" + MainClass.descargasEnProceso [i].url + "</div>" +
					"<div class=\"p\"><div class=\"progressBar\"><div style=\"width:" + MainClass.descargasEnProceso [i].porcentaje.ToString ().Replace (",", ".") + "%\"></div></div>" + MainClass.descargasEnProceso [i].porcentajeInt + "%</div>" +
					"<div class=\"t\">" + MainClass.descargasEnProceso [i].horaRestanteString + "</div>" +
					"<div class=\"q\"><a href=\"/?accion=cancelarDescarga&elem=" + i + "\">Quitar</a></div>" +
					"</div>";
			}
		}
		
		if(mensaje != ""){
			resp += "<script>alert(\""+mensaje+"\")</script>";
		}
		
		return resp;
	}

	public static string cierraConJS(){
		return "<html><body>"+
			"La descarga ha sido agregada.<br>"+
			"<a href='/'>Clica aquí para ver el progreso de las descargas.</a> o cierra esta pestaña."+
			"</body></html>";
	}

	public static string getF4Mdownloaderjs(){
		return "f4mdownloader=true;";
	}

	public static string getAyuda(){
		return getCabeceraHTML() +
				"<div id=\"contenido\">" +
					"<img src=\"/ayuda/ayuda_prev.png\" class=\"img_ayuda\">" +
					"<ol>" +
						"<li>Versión del programa." +
							"<br>En el caso de la imagen se trata de la versión 0.0.1.<br>" +
							"Si queremos buscar actualizaciones, se entenderá como una versión superior aquella que tenga un número mayor que el actual, siendo por ejemplo la versión 2.0 superior a la 1.5 y a su vez superior a la versión 0.3.</li>" +
						"<li>Enlace a la sección de versiones del programa en Descargavideos.<br>" +
							"Al hacer clic en el enlace se abrirá una página con el listado de versiones publicadas donde podrás descargar la más reciente o cualquiera de las versiones anteriores.</li>" +
						"<li>Formulario para agregar nuevas descargas.<br>" +
							"En caso de querer agregar manualmente una descarga, introduce la url del archivo f4m siguiendo el patrón (--manifest \"URL\"), sin los paréntesis y cambiando URL por la url del archivo f4m. Eescribe el nombre que quieres que tenga el vídeo en el campo Nombre (En caso de no indicar un nombre se usara el nombre especificado en el comando). El nombre del vídeo debe tener la extensión del archivo, siendo un nombre válido por ejemplo <i>video</i> o <i>capítulo 15</i>. El nombre puede dejarse en blanco.<br>Una vez completado el formulario al clicar <i>Agregar</i> comenzará la descarga del vídeo.</li>" +
						"<li>Nombre del archivo tal y como figura en la carpeta que lo contiene.</li>" +
						"<li>URL del manifest F4M que contiene las indicaciones para descargar el vídeo.</li>" +
						"<li>Progreso de la descarga.<br>" +
							"Una vez completada cada descarga se abrirá la carpeta que contiene el vídeo. Hasta entonces, no debe moverse o borrarse el archivo.</li>" +
						"<li>Tiempo restante para completar la descarga.<br>" +
							"El tiempo es calculado a partir del porcentaje descargado y el tiempo transcurrido por lo que únicamente es aproximado.</li>" +
						"<li>Quitar la descarga de la lista.<br>" +
							"En caso de que la descarga no esté finalizada detendrá la descarga y la quitará de la lista, dejando el archivo incompleto en la carpeta de descargas.<br>" +
							"En caso de que la descarga esté finalizada, únicamente la quitará de la lista.</li>" +
						"<li>Cerrar el programa.<br>" +
							"Para cerrar el programa se debe de hacer clic en el botón. Al hacerlo, se mostrará una pregunta. En caso de que aceptemos, todas las descargas incompletas en curso se interrumpirán y después se cerrará el programa. De lo contrario no se cerrará el programa. Una vez hecho esto cargará una nueva página en la que indicará que el programa se ha cerrado con éxito.<br>En caso de cerrar la consola (la ventana negra) en lugar de clicar en el botón de cerrar dejará las descargas en proceso abiertas, por lo que para detenerlas sería necesario cerrar el proceso en cuestión.</li>" +
					"</ol>" +
					"<a href=\"/\">atras</a>" +
				"</div>" +
			"</body>" +
			"</html>";
	}
	
	public static string getSeleccionLista(string opciones){
		return "<html><body><h3>Se han encontrado varias opciones de descarga</h3><br>A mayor BANDWIDTH Y/O RESOLUTION, mayor calidad de imagen<br>Por favor, clica en la opción que quieras descargar:<br><br><br>"+opciones;
	}

	public static string getCerrado(){
		return "Has cerrado el programa<br>Ahora puedes cerrar esta ventana.";
	}

	public static string getFileBrowser(string dirPath){
		string respuesta = getCabeceraHTML () + "<div id=\"contenido\">";
		try{
			respuesta += "<div class='tabla'>";
			respuesta += "<div class='elemento'><div>Ruta actual</div><div>"+dirPath+"</div></div>";

			respuesta += "<div class='elemento'><div>Discos duros</div><div>";

			DriveInfo[] allDrives = DriveInfo.GetDrives();
			foreach (DriveInfo d in allDrives)
			{
				if (d.IsReady == true)
				{
					respuesta += "<a href='/listadirs?ruta="+d.Name+"\\'>"+d.VolumeLabel+"</a> | ";
				}
			}

			respuesta += "</div></div>";

			respuesta += "<div class='elemento'><div></div><div><a href='/elijedir?ruta="+dirPath+"'>Guardar los vídeos en esta carpeta</a></div></div>";

			respuesta += "</div><br>";



			if(dirPath.Length > 4){
				respuesta += "<a href='/listadirs?ruta="+dirPath.Substring(0, dirPath.LastIndexOf("\\",dirPath.Length -2) +1)+"'><div class='directorio'><img src='/listadirs/folder.png'><br>Carpeta superior</div></a>";
			}

			dirPath = dirPath.Substring(0, dirPath.Length -1);
			List<string> dirs = new List<string>(System.IO.Directory.GetDirectories(dirPath));

            foreach (var dir in dirs)
            {
				string d = dir.Substring(dir.LastIndexOf("\\") + 1);
				respuesta += "<a href='/listadirs?ruta="+dirPath+"\\"+d+"\\'><div class='directorio'><img src='/listadirs/folder.png'><br>"+d+"</div></a>";
            }
		}
		catch(Exception e){
			//return e.ToString();
		}
		return respuesta + "</div>" +
			"</body>" +
			"</html>";
	}
}